using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderOptExprProp
    {
        private struct UseSite
        {
            public ShaderIrNode Parent { get; private set; }
            public ShaderIrCond Cond   { get; private set; }

            public int UseIndex { get; private set; }

            public int OperIndex { get; private set; }

            public UseSite(
                ShaderIrNode Parent,
                ShaderIrCond Cond,
                int          UseIndex,
                int          OperIndex)
            {
                this.Parent    = Parent;
                this.Cond      = Cond;
                this.UseIndex  = UseIndex;
                this.OperIndex = OperIndex;
            }
        }

        private class RegUse
        {
            public ShaderIrAsg Asg { get; private set; }

            public int AsgIndex { get; private set; }

            public int LastSiteIndex { get; private set; }

            public ShaderIrCond Cond { get; private set; }

            private List<UseSite> Sites;

            public RegUse()
            {
                Sites = new List<UseSite>();
            }

            public void AddUseSite(UseSite Site)
            {
                if (LastSiteIndex < Site.UseIndex)
                {
                    LastSiteIndex = Site.UseIndex;
                }

                Sites.Add(Site);
            }

            public bool TryPropagate()
            {
                //This happens when a untiliazied register is used,
                //this usually indicates a decoding error, but may also
                //be caused by bogus programs (?). In any case, we just
                //keep the unitialized access and avoid trying to propagate
                //the expression (since we can't propagate what doesn't yet exist).
                if (Asg == null)
                {
                    return false;
                }

                if (Cond != null)
                {
                    //If the assignment is conditional, we can only propagate
                    //to the use sites that shares the same condition of the assignment.
                    foreach (UseSite Site in Sites)
                    {
                        if (!IsSameCondition(Cond, Site.Cond))
                        {
                            return false;
                        }
                    }
                }

                if (Sites.Count > 0)
                {
                    foreach (UseSite Site in Sites)
                    {
                        if (Site.Parent is ShaderIrCond Cond)
                        {
                            switch (Site.OperIndex)
                            {
                                case 0: Cond.Pred  = Asg.Src; break;
                                case 1: Cond.Child = Asg.Src; break;

                                default: throw new InvalidOperationException();
                            }
                        }
                        else if (Site.Parent is ShaderIrOp Op)
                        {
                            switch (Site.OperIndex)
                            {
                                case 0: Op.OperandA = Asg.Src; break;
                                case 1: Op.OperandB = Asg.Src; break;
                                case 2: Op.OperandC = Asg.Src; break;

                                default: throw new InvalidOperationException();
                            }
                        }
                        else if (Site.Parent is ShaderIrAsg SiteAsg)
                        {
                            SiteAsg.Src = Asg.Src;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }

                return true;
            }

            public void SetNewAsg(ShaderIrAsg Asg, int AsgIndex, ShaderIrCond Cond)
            {
                this.Asg      = Asg;
                this.AsgIndex = AsgIndex;
                this.Cond     = Cond;

                LastSiteIndex = 0;

                Sites.Clear();
            }
        }

        public static void Optimize(List<ShaderIrNode> Nodes, GalShaderType ShaderType)
        {
            Dictionary<int, RegUse> Uses = new Dictionary<int, RegUse>();

            RegUse GetUse(int Key)
            {
                RegUse Use;

                if (!Uses.TryGetValue(Key, out Use))
                {
                    Use = new RegUse();

                    Uses.Add(Key, Use);
                }

                return Use;
            }

            int GetGprKey(int GprIndex)
            {
                return GprIndex;
            }

            int GetPredKey(int PredIndex)
            {
                return PredIndex | 0x10000000;
            }

            RegUse GetGprUse(int GprIndex)
            {
                return GetUse(GetGprKey(GprIndex));
            }

            RegUse GetPredUse(int PredIndex)
            {
                return GetUse(GetPredKey(PredIndex));
            }

            void RemoveUse(RegUse Use)
            {
                if (!Nodes.Remove((ShaderIrNode)Use.Cond ?? Use.Asg))
                {
                    throw new InvalidOperationException();
                }
            }

            void FindRegUses(
                List<(int, UseSite)> UseList,
                ShaderIrNode         Parent,
                ShaderIrNode         Node,
                ShaderIrCond         CondNode,
                int                  UseIndex,
                int                  OperIndex = 0)
            {
                if (Node is ShaderIrAsg Asg)
                {
                    FindRegUses(UseList, Asg, Asg.Src, CondNode, UseIndex);
                }
                else if (Node is ShaderIrCond Cond)
                {
                    FindRegUses(UseList, Cond, Cond.Pred,  CondNode, UseIndex, 0);
                    FindRegUses(UseList, Cond, Cond.Child, CondNode, UseIndex, 1);
                }
                else if (Node is ShaderIrOp Op)
                {
                    FindRegUses(UseList, Op, Op.OperandA, CondNode, UseIndex, 0);
                    FindRegUses(UseList, Op, Op.OperandB, CondNode, UseIndex, 1);
                    FindRegUses(UseList, Op, Op.OperandC, CondNode, UseIndex, 2);
                }
                else if (Node is ShaderIrOperGpr Gpr && !Gpr.IsConst)
                {
                    UseList.Add((GetGprKey(Gpr.Index), new UseSite(Parent, CondNode, UseIndex, OperIndex)));
                }
                else if (Node is ShaderIrOperPred Pred)
                {
                    UseList.Add((GetPredKey(Pred.Index), new UseSite(Parent, CondNode, UseIndex, OperIndex)));
                }
            }

            void TryAddRegUseSite(ShaderIrNode Node, ShaderIrCond CondNode, int UseIndex)
            {
                List<(int, UseSite)> UseList = new List<(int, UseSite)>();

                FindRegUses(UseList, null, Node, CondNode, UseIndex);

                foreach ((int Key, UseSite Site) in UseList)
                {
                    GetUse(Key).AddUseSite(Site);
                }
            }

            bool TryPropagate(RegUse Use)
            {
                //We can only propagate if the registers that the expression depends
                //on weren't assigned after the original expression assignment
                //to a register took place. We traverse the expression tree to find
                //all registers being used, if any of those registers was assigned
                //after the assignment to be propagated, then we can't propagate.
                if (Use?.Asg == null)
                {
                    return false;
                }

                List<(int, UseSite)> UseList = new List<(int, UseSite)>();

                if (Use.Cond != null)
                {
                    FindRegUses(UseList, null, Use.Cond, null, 0);
                }
                else
                {
                    FindRegUses(UseList, Use.Asg, Use.Asg.Src, null, 0);
                }

                foreach ((int Key, UseSite Site) in UseList)
                {
                    //TODO: Build an assignment list inside RegUse,
                    //and check if there is an assignment inside the
                    //range of Use.AsgIndex and Use.LastSiteIndex,
                    //and if that's the case, then we should return false.
                    //The current method is too conservative.
                    if (GetUse(Key).AsgIndex >= Use.AsgIndex)
                    {
                        return false;
                    }
                }

                return Use.TryPropagate();
            }

            for (int Index = 0, IterCount = 0; Index < Nodes.Count; Index++, IterCount++)
            {
                ShaderIrNode Node = Nodes[Index];

                ShaderIrCond CondNode = null;

                if (Node is ShaderIrCond)
                {
                    CondNode = (ShaderIrCond)Node;
                }

                TryAddRegUseSite(Node, CondNode, IterCount);;

                while (Node is ShaderIrCond Cond)
                {
                    Node = Cond.Child;
                }

                if (!(Node is ShaderIrAsg Asg))
                {
                    continue;
                }

                RegUse Use = null;

                if (Asg.Dst is ShaderIrOperGpr Gpr && !Gpr.IsConst)
                {
                    Use = GetGprUse(Gpr.Index);
                }
                else if (Asg.Dst is ShaderIrOperPred Pred)
                {
                    Use = GetPredUse(Pred.Index);
                }

                bool CanRemoveAsg = CondNode == null;

                CanRemoveAsg |= IsSameCondition(CondNode, Use?.Cond);

                if (CanRemoveAsg && TryPropagate(Use))
                {
                    RemoveUse(Use);

                    //Note: Only decrement if the removal was successful.
                    //RemoveUse throws when this is not the case so we should be good.
                    Index--;
                }

                //All nodes inside conditional nodes can't be propagated,
                //as we don't even know if they will be executed to begin with.
                Use?.SetNewAsg(Asg, IterCount, CondNode);
            }

            foreach (RegUse Use in Uses.Values)
            {
                //Gprs 0-3 are the color output on fragment shaders,
                //so we can't remove the last assignments to those registers.
                if (ShaderType == GalShaderType.Fragment)
                {
                    if (Use.Asg?.Dst is ShaderIrOperGpr Gpr && Gpr.Index < 4)
                    {
                        continue;
                    }
                }

                if (TryPropagate(Use))
                {
                    RemoveUse(Use);
                }
            }
        }

        private static bool IsSameCondition(ShaderIrCond CondA, ShaderIrCond CondB)
        {
            if (CondA == null || CondB == null)
            {
                return CondA == CondB;
            }

            if (CondA.Not != CondB.Not)
            {
                return false;
            }

            if (CondA.Pred is ShaderIrOperPred PredA)
            {
                if (!(CondB.Pred is ShaderIrOperPred PredB))
                {
                    return false;
                }

                if (PredA.Index != PredB.Index)
                {
                    return false;
                }
            }
            else if (CondA.Pred != CondB.Pred)
            {
                return false;
            }

            return true;
        }
    }
}
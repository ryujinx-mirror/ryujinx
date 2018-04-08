using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderOptExprProp
    {
        private struct UseSite
        {
            public object Parent;

            public int OperIndex;

            public UseSite(object Parent, int OperIndex)
            {
                this.Parent    = Parent;
                this.OperIndex = OperIndex;
            }
        }

        private class RegUse
        {
            public ShaderIrAsg Asg { get; private set; }

            public int AsgIndex { get; private set; }

            private bool Propagate;

            private List<UseSite> Sites;

            public RegUse()
            {
                Sites = new List<UseSite>();
            }

            public void AddUseSite(UseSite Site)
            {
                Sites.Add(Site);
            }

            public bool TryPropagate()
            {
                //This happens when a untiliazied register is used,
                //this usually indicates a decoding error, but may also
                //be cased by bogus programs (?). In any case, we just
                //keep the unitialized access and avoid trying to propagate
                //the expression (since we can't propagate what doesn't yet exist).
                if (Asg == null || !Propagate)
                {
                    return false;
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

            public void SetNewAsg(ShaderIrAsg Asg, int AsgIndex, bool Propagate)
            {
                this.Asg       = Asg;
                this.AsgIndex  = AsgIndex;
                this.Propagate = Propagate;

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

            void FindRegUses(List<(int, UseSite)> UseList, object Parent, ShaderIrNode Node, int OperIndex = 0)
            {
                if (Node is ShaderIrAsg Asg)
                {
                    FindRegUses(UseList, Asg, Asg.Src);
                }
                else if (Node is ShaderIrCond Cond)
                {
                    FindRegUses(UseList, Cond, Cond.Pred,  0);
                    FindRegUses(UseList, Cond, Cond.Child, 1);
                }
                else if (Node is ShaderIrOp Op)
                {
                    FindRegUses(UseList, Op, Op.OperandA, 0);
                    FindRegUses(UseList, Op, Op.OperandB, 1);
                    FindRegUses(UseList, Op, Op.OperandC, 2);
                }
                else if (Node is ShaderIrOperGpr Gpr && Gpr.Index != ShaderIrOperGpr.ZRIndex)
                {
                    UseList.Add((GetGprKey(Gpr.Index), new UseSite(Parent, OperIndex)));
                }
                else if (Node is ShaderIrOperPred Pred)
                {
                    UseList.Add((GetPredKey(Pred.Index), new UseSite(Parent, OperIndex)));
                }
            }

            void TryAddRegUseSite(ShaderIrNode Node)
            {
                List<(int, UseSite)> UseList = new List<(int, UseSite)>();

                FindRegUses(UseList, null, Node);

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

                FindRegUses(UseList, Use.Asg, Use.Asg.Src);

                foreach ((int Key, UseSite Site) in UseList)
                {
                    if (GetUse(Key).AsgIndex >= Use.AsgIndex)
                    {
                        return false;
                    }
                }

                return Use.TryPropagate();
            }

            for (int Index = 0, AsgIndex = 0; Index < Nodes.Count; Index++, AsgIndex++)
            {
                ShaderIrNode Node = Nodes[Index];

                bool IsConditional = Node is ShaderIrCond;

                TryAddRegUseSite(Node);

                while (Node is ShaderIrCond Cond)
                {
                    Node = Cond.Child;
                }

                if (!(Node is ShaderIrAsg Asg))
                {
                    continue;
                }

                RegUse Use = null;

                if (Asg.Dst is ShaderIrOperGpr Gpr && Gpr.Index != ShaderIrOperGpr.ZRIndex)
                {
                    Use = GetGprUse(Gpr.Index);
                }
                else if (Asg.Dst is ShaderIrOperPred Pred)
                {
                    Use = GetPredUse(Pred.Index);
                }

                if (!IsConditional && TryPropagate(Use))
                {
                    Nodes.Remove(Use.Asg);

                    Index--;
                }

                //All nodes inside conditional nodes can't be propagated,
                //as we don't even know if they will be executed to begin with.
                Use?.SetNewAsg(Asg, AsgIndex, !IsConditional);
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
                    Nodes.Remove(Use.Asg);
                }
            }
        }
    }
}
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Translation.Transforms
{
    static class TransformPasses
    {
        public static void RunPass(TransformContext context)
        {
            RunPass<DrawParametersReplace>(context);
            RunPass<ForcePreciseEnable>(context);
            RunPass<VectorComponentSelect>(context);
            RunPass<TexturePass>(context);
            RunPass<SharedStoreSmallIntCas>(context);
            RunPass<SharedAtomicSignedCas>(context);
            RunPass<ShufflePass>(context);
            RunPass<VertexToCompute>(context);
            RunPass<GeometryToCompute>(context);
        }

        private static void RunPass<T>(TransformContext context) where T : ITransformPass
        {
            if (!T.IsEnabled(context.GpuAccessor, context.Stage, context.TargetLanguage, context.UsedFeatures))
            {
                return;
            }

            for (int blkIndex = 0; blkIndex < context.Blocks.Length; blkIndex++)
            {
                BasicBlock block = context.Blocks[blkIndex];

                for (LinkedListNode<INode> node = block.Operations.First; node != null; node = node.Next)
                {
                    if (node.Value is not Operation)
                    {
                        continue;
                    }

                    node = T.RunPass(context, node);
                }
            }
        }
    }
}

using ChocolArm64.State;
using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    public class CpuTestSimdArithmetic : CpuTest
    {
    	[TestCase(0x3FE66666u, 'N', false, 0x40000000u)]
    	[TestCase(0x3F99999Au, 'N', false, 0x3F800000u)]
    	[TestCase(0x404CCCCDu, 'P', false, 0x40800000u)]
    	[TestCase(0x40733333u, 'P', false, 0x40800000u)]
    	[TestCase(0x404CCCCDu, 'M', false, 0x40400000u)]
    	[TestCase(0x40733333u, 'M', false, 0x40400000u)]
    	[TestCase(0x3F99999Au, 'Z', false, 0x3F800000u)]
    	[TestCase(0x3FE66666u, 'Z', false, 0x3F800000u)]
    	[TestCase(0x00000000u, 'N', false, 0x00000000u)]
    	[TestCase(0x00000000u, 'P', false, 0x00000000u)]
    	[TestCase(0x00000000u, 'M', false, 0x00000000u)]
    	[TestCase(0x00000000u, 'Z', false, 0x00000000u)]
    	[TestCase(0x80000000u, 'N', false, 0x80000000u)]
    	[TestCase(0x80000000u, 'P', false, 0x80000000u)]
    	[TestCase(0x80000000u, 'M', false, 0x80000000u)]
    	[TestCase(0x80000000u, 'Z', false, 0x80000000u)]
    	[TestCase(0x7F800000u, 'N', false, 0x7F800000u)]
    	[TestCase(0x7F800000u, 'P', false, 0x7F800000u)]
    	[TestCase(0x7F800000u, 'M', false, 0x7F800000u)]
    	[TestCase(0x7F800000u, 'Z', false, 0x7F800000u)]
    	[TestCase(0xFF800000u, 'N', false, 0xFF800000u)]
    	[TestCase(0xFF800000u, 'P', false, 0xFF800000u)]
    	[TestCase(0xFF800000u, 'M', false, 0xFF800000u)]
    	[TestCase(0xFF800000u, 'Z', false, 0xFF800000u)]
    	[TestCase(0xFF800001u, 'N', false, 0xFFC00001u)]
    	[TestCase(0xFF800001u, 'P', false, 0xFFC00001u)]
    	[TestCase(0xFF800001u, 'M', false, 0xFFC00001u)]
    	[TestCase(0xFF800001u, 'Z', false, 0xFFC00001u)]
    	[TestCase(0xFF800001u, 'N', true,  0x7FC00000u)]
    	[TestCase(0xFF800001u, 'P', true,  0x7FC00000u)]
    	[TestCase(0xFF800001u, 'M', true,  0x7FC00000u)]
    	[TestCase(0xFF800001u, 'Z', true,  0x7FC00000u)]
    	[TestCase(0x7FC00002u, 'N', false, 0x7FC00002u)]
    	[TestCase(0x7FC00002u, 'P', false, 0x7FC00002u)]
    	[TestCase(0x7FC00002u, 'M', false, 0x7FC00002u)]
    	[TestCase(0x7FC00002u, 'Z', false, 0x7FC00002u)]
    	[TestCase(0x7FC00002u, 'N', true,  0x7FC00000u)]
    	[TestCase(0x7FC00002u, 'P', true,  0x7FC00000u)]
    	[TestCase(0x7FC00002u, 'M', true,  0x7FC00000u)]
    	[TestCase(0x7FC00002u, 'Z', true,  0x7FC00000u)]
    	public void Frintx_S(uint A, char RoundType, bool DefaultNaN, uint Result)
    	{
        	int FpcrTemp = 0x0;
        	switch(RoundType)
        	{
        		case 'N':
        		FpcrTemp = 0x0;
        		break;

        		case 'P':
        		FpcrTemp = 0x400000;
        		break;

        		case 'M':
        		FpcrTemp = 0x800000;
        		break;

        		case 'Z':
        		FpcrTemp = 0xC00000;
        		break;
        	}
        	if(DefaultNaN)
        	{
        		FpcrTemp |= 1 << 25;
        	}
        	AVec V1 = new AVec { X0 = A };
        	AThreadState ThreadState = SingleOpcode(0x1E274020, V1: V1, Fpcr: FpcrTemp);
        	Assert.AreEqual(Result, ThreadState.V0.X0);
        }
    }
}

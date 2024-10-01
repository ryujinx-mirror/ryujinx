namespace Ryujinx.Cpu.LightningJit.Arm32
{
    readonly struct InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm12b0w12
    {
        private readonly uint _value;
        public InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstIb26w1Sb20w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8
    {
        private readonly uint _value;
        public InstIb26w1Sb20w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint I => (_value >> 26) & 0x1;
    }

    readonly struct InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 5) & 0x3;
        public readonly uint Imm5 => (_value >> 7) & 0x1F;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRmb19w3Rdnb16w3
    {
        private readonly uint _value;
        public InstRmb19w3Rdnb16w3(uint value) => _value = value;
        public readonly uint Rdn => (_value >> 16) & 0x7;
        public readonly uint Rm => (_value >> 19) & 0x7;
    }

    readonly struct InstSb20w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4
    {
        private readonly uint _value;
        public InstSb20w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 4) & 0x3;
        public readonly uint Imm2 => (_value >> 6) & 0x3;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
    }

    readonly struct InstCondb28w4Sb20w1Rnb16w4Rdb12w4Rsb8w4Stypeb5w2Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Sb20w1Rnb16w4Rdb12w4Rsb8w4Stypeb5w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 5) & 0x3;
        public readonly uint Rs => (_value >> 8) & 0xF;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstImm3b22w3Rnb19w3Rdb16w3
    {
        private readonly uint _value;
        public InstImm3b22w3Rnb19w3Rdb16w3(uint value) => _value = value;
        public readonly uint Rd => (_value >> 16) & 0x7;
        public readonly uint Rn => (_value >> 19) & 0x7;
        public readonly uint Imm3 => (_value >> 22) & 0x7;
    }

    readonly struct InstRdnb24w3Imm8b16w8
    {
        private readonly uint _value;
        public InstRdnb24w3Imm8b16w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 16) & 0xFF;
        public readonly uint Rdn => (_value >> 24) & 0x7;
    }

    readonly struct InstIb26w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8
    {
        private readonly uint _value;
        public InstIb26w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint I => (_value >> 26) & 0x1;
    }

    readonly struct InstRmb22w3Rnb19w3Rdb16w3
    {
        private readonly uint _value;
        public InstRmb22w3Rnb19w3Rdb16w3(uint value) => _value = value;
        public readonly uint Rd => (_value >> 16) & 0x7;
        public readonly uint Rn => (_value >> 19) & 0x7;
        public readonly uint Rm => (_value >> 22) & 0x7;
    }

    readonly struct InstDnb23w1Rmb19w4Rdnb16w3
    {
        private readonly uint _value;
        public InstDnb23w1Rmb19w4Rdnb16w3(uint value) => _value = value;
        public readonly uint Rdn => (_value >> 16) & 0x7;
        public readonly uint Rm => (_value >> 19) & 0xF;
        public readonly uint Dn => (_value >> 23) & 0x1;
    }

    readonly struct InstCondb28w4Sb20w1Rdb12w4Imm12b0w12
    {
        private readonly uint _value;
        public InstCondb28w4Sb20w1Rdb12w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRdb24w3Imm8b16w8
    {
        private readonly uint _value;
        public InstRdb24w3Imm8b16w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 16) & 0xFF;
        public readonly uint Rd => (_value >> 24) & 0x7;
    }

    readonly struct InstImm7b16w7
    {
        private readonly uint _value;
        public InstImm7b16w7(uint value) => _value = value;
        public readonly uint Imm7 => (_value >> 16) & 0x7F;
    }

    readonly struct InstIb26w1Sb20w1Imm3b12w3Rdb8w4Imm8b0w8
    {
        private readonly uint _value;
        public InstIb26w1Sb20w1Imm3b12w3Rdb8w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint I => (_value >> 26) & 0x1;
    }

    readonly struct InstIb26w1Imm3b12w3Rdb8w4Imm8b0w8
    {
        private readonly uint _value;
        public InstIb26w1Imm3b12w3Rdb8w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint I => (_value >> 26) & 0x1;
    }

    readonly struct InstCondb28w4Sb20w1Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Sb20w1Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 5) & 0x3;
        public readonly uint Imm5 => (_value >> 7) & 0x1F;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstDmb23w1Rdmb16w3
    {
        private readonly uint _value;
        public InstDmb23w1Rdmb16w3(uint value) => _value = value;
        public readonly uint Rdm => (_value >> 16) & 0x7;
        public readonly uint Dm => (_value >> 23) & 0x1;
    }

    readonly struct InstRmb19w4
    {
        private readonly uint _value;
        public InstRmb19w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 19) & 0xF;
    }

    readonly struct InstSb20w1Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4
    {
        private readonly uint _value;
        public InstSb20w1Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 4) & 0x3;
        public readonly uint Imm2 => (_value >> 6) & 0x3;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint S => (_value >> 20) & 0x1;
    }

    readonly struct InstCondb28w4Rdb12w4Imm12b0w12
    {
        private readonly uint _value;
        public InstCondb28w4Rdb12w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Size => (_value >> 18) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstCondb28w4Imm24b0w24
    {
        private readonly uint _value;
        public InstCondb28w4Imm24b0w24(uint value) => _value = value;
        public readonly uint Imm24 => (_value >> 0) & 0xFFFFFF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstCondb24w4Imm8b16w8
    {
        private readonly uint _value;
        public InstCondb24w4Imm8b16w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 16) & 0xFF;
        public readonly uint Cond => (_value >> 24) & 0xF;
    }

    readonly struct InstImm11b16w11
    {
        private readonly uint _value;
        public InstImm11b16w11(uint value) => _value = value;
        public readonly uint Imm11 => (_value >> 16) & 0x7FF;
    }

    readonly struct InstSb26w1Condb22w4Imm6b16w6J1b13w1J2b11w1Imm11b0w11
    {
        private readonly uint _value;
        public InstSb26w1Condb22w4Imm6b16w6J1b13w1J2b11w1Imm11b0w11(uint value) => _value = value;
        public readonly uint Imm11 => (_value >> 0) & 0x7FF;
        public readonly uint J2 => (_value >> 11) & 0x1;
        public readonly uint J1 => (_value >> 13) & 0x1;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint Cond => (_value >> 22) & 0xF;
        public readonly uint S => (_value >> 26) & 0x1;
    }

    readonly struct InstSb26w1Imm10b16w10J1b13w1J2b11w1Imm11b0w11
    {
        private readonly uint _value;
        public InstSb26w1Imm10b16w10J1b13w1J2b11w1Imm11b0w11(uint value) => _value = value;
        public readonly uint Imm11 => (_value >> 0) & 0x7FF;
        public readonly uint J2 => (_value >> 11) & 0x1;
        public readonly uint J1 => (_value >> 13) & 0x1;
        public readonly uint Imm10 => (_value >> 16) & 0x3FF;
        public readonly uint S => (_value >> 26) & 0x1;
    }

    readonly struct InstCondb28w4Msbb16w5Rdb12w4Lsbb7w5
    {
        private readonly uint _value;
        public InstCondb28w4Msbb16w5Rdb12w4Lsbb7w5(uint value) => _value = value;
        public readonly uint Lsb => (_value >> 7) & 0x1F;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Msb => (_value >> 16) & 0x1F;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstImm3b12w3Rdb8w4Imm2b6w2Msbb0w5
    {
        private readonly uint _value;
        public InstImm3b12w3Rdb8w4Imm2b6w2Msbb0w5(uint value) => _value = value;
        public readonly uint Msb => (_value >> 0) & 0x1F;
        public readonly uint Imm2 => (_value >> 6) & 0x3;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
    }

    readonly struct InstCondb28w4Msbb16w5Rdb12w4Lsbb7w5Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Msbb16w5Rdb12w4Lsbb7w5Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint Lsb => (_value >> 7) & 0x1F;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Msb => (_value >> 16) & 0x1F;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Imm3b12w3Rdb8w4Imm2b6w2Msbb0w5
    {
        private readonly uint _value;
        public InstRnb16w4Imm3b12w3Rdb8w4Imm2b6w2Msbb0w5(uint value) => _value = value;
        public readonly uint Msb => (_value >> 0) & 0x1F;
        public readonly uint Imm2 => (_value >> 6) & 0x3;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Imm12b8w12Imm4b0w4
    {
        private readonly uint _value;
        public InstCondb28w4Imm12b8w12Imm4b0w4(uint value) => _value = value;
        public readonly uint Imm4 => (_value >> 0) & 0xF;
        public readonly uint Imm12 => (_value >> 8) & 0xFFF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstImm8b16w8
    {
        private readonly uint _value;
        public InstImm8b16w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 16) & 0xFF;
    }

    readonly struct InstCondb28w4Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstHb24w1Imm24b0w24
    {
        private readonly uint _value;
        public InstHb24w1Imm24b0w24(uint value) => _value = value;
        public readonly uint Imm24 => (_value >> 0) & 0xFFFFFF;
        public readonly uint H => (_value >> 24) & 0x1;
    }

    readonly struct InstSb26w1Imm10hb16w10J1b13w1J2b11w1Imm10lb1w10Hb0w1
    {
        private readonly uint _value;
        public InstSb26w1Imm10hb16w10J1b13w1J2b11w1Imm10lb1w10Hb0w1(uint value) => _value = value;
        public readonly uint H => (_value >> 0) & 0x1;
        public readonly uint Imm10l => (_value >> 1) & 0x3FF;
        public readonly uint J2 => (_value >> 11) & 0x1;
        public readonly uint J1 => (_value >> 13) & 0x1;
        public readonly uint Imm10h => (_value >> 16) & 0x3FF;
        public readonly uint S => (_value >> 26) & 0x1;
    }

    readonly struct InstRmb16w4
    {
        private readonly uint _value;
        public InstRmb16w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 16) & 0xF;
    }

    readonly struct InstOpb27w1Ib25w1Imm5b19w5Rnb16w3
    {
        private readonly uint _value;
        public InstOpb27w1Ib25w1Imm5b19w5Rnb16w3(uint value) => _value = value;
        public readonly uint Rn => (_value >> 16) & 0x7;
        public readonly uint Imm5 => (_value >> 19) & 0x1F;
        public readonly uint I => (_value >> 25) & 0x1;
        public readonly uint Op => (_value >> 27) & 0x1;
    }

    readonly struct InstCondb28w4
    {
        private readonly uint _value;
        public InstCondb28w4(uint value) => _value = value;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstCondb28w4Rdb12w4Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdb12w4Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rdb8w4Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rdb8w4Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rnb16w4Imm12b0w12
    {
        private readonly uint _value;
        public InstCondb28w4Rnb16w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstIb26w1Rnb16w4Imm3b12w3Imm8b0w8
    {
        private readonly uint _value;
        public InstIb26w1Rnb16w4Imm3b12w3Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint I => (_value >> 26) & 0x1;
    }

    readonly struct InstCondb28w4Rnb16w4Imm5b7w5Stypeb5w2Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rnb16w4Imm5b7w5Stypeb5w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 5) & 0x3;
        public readonly uint Imm5 => (_value >> 7) & 0x1F;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRmb19w3Rnb16w3
    {
        private readonly uint _value;
        public InstRmb19w3Rnb16w3(uint value) => _value = value;
        public readonly uint Rn => (_value >> 16) & 0x7;
        public readonly uint Rm => (_value >> 19) & 0x7;
    }

    readonly struct InstRnb16w4Imm3b12w3Imm2b6w2Stypeb4w2Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Imm3b12w3Imm2b6w2Stypeb4w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 4) & 0x3;
        public readonly uint Imm2 => (_value >> 6) & 0x3;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rnb16w4Rsb8w4Stypeb5w2Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rnb16w4Rsb8w4Stypeb5w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 5) & 0x3;
        public readonly uint Rs => (_value >> 8) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb24w3Imm8b16w8
    {
        private readonly uint _value;
        public InstRnb24w3Imm8b16w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 16) & 0xFF;
        public readonly uint Rn => (_value >> 24) & 0x7;
    }

    readonly struct InstNb23w1Rmb19w4Rnb16w3
    {
        private readonly uint _value;
        public InstNb23w1Rmb19w4Rnb16w3(uint value) => _value = value;
        public readonly uint Rn => (_value >> 16) & 0x7;
        public readonly uint Rm => (_value >> 19) & 0xF;
        public readonly uint N => (_value >> 23) & 0x1;
    }

    readonly struct InstImodb18w2Mb17w1Ab8w1Ib7w1Fb6w1Modeb0w5
    {
        private readonly uint _value;
        public InstImodb18w2Mb17w1Ab8w1Ib7w1Fb6w1Modeb0w5(uint value) => _value = value;
        public readonly uint Mode => (_value >> 0) & 0x1F;
        public readonly uint F => (_value >> 6) & 0x1;
        public readonly uint I => (_value >> 7) & 0x1;
        public readonly uint A => (_value >> 8) & 0x1;
        public readonly uint M => (_value >> 17) & 0x1;
        public readonly uint Imod => (_value >> 18) & 0x3;
    }

    readonly struct InstImb20w1Ab18w1Ib17w1Fb16w1
    {
        private readonly uint _value;
        public InstImb20w1Ab18w1Ib17w1Fb16w1(uint value) => _value = value;
        public readonly uint F => (_value >> 16) & 0x1;
        public readonly uint I => (_value >> 17) & 0x1;
        public readonly uint A => (_value >> 18) & 0x1;
        public readonly uint Im => (_value >> 20) & 0x1;
    }

    readonly struct InstImodb9w2Mb8w1Ab7w1Ib6w1Fb5w1Modeb0w5
    {
        private readonly uint _value;
        public InstImodb9w2Mb8w1Ab7w1Ib6w1Fb5w1Modeb0w5(uint value) => _value = value;
        public readonly uint Mode => (_value >> 0) & 0x1F;
        public readonly uint F => (_value >> 5) & 0x1;
        public readonly uint I => (_value >> 6) & 0x1;
        public readonly uint A => (_value >> 7) & 0x1;
        public readonly uint M => (_value >> 8) & 0x1;
        public readonly uint Imod => (_value >> 9) & 0x3;
    }

    readonly struct InstCondb28w4Szb21w2Rnb16w4Rdb12w4Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Szb21w2Rnb16w4Rdb12w4Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint Sz => (_value >> 21) & 0x3;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rdb8w4Szb4w2Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rdb8w4Szb4w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Sz => (_value >> 4) & 0x3;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Optionb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Optionb0w4(uint value) => _value = value;
        public readonly uint Option => (_value >> 0) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstOptionb0w4
    {
        private readonly uint _value;
        public InstOptionb0w4(uint value) => _value = value;
        public readonly uint Option => (_value >> 0) & 0xF;
    }

    readonly struct InstCondb28w4Pb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm871b1w7
    {
        private readonly uint _value;
        public InstCondb28w4Pb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm871b1w7(uint value) => _value = value;
        public readonly uint Imm871 => (_value >> 1) & 0x7F;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstPb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm871b1w7
    {
        private readonly uint _value;
        public InstPb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm871b1w7(uint value) => _value = value;
        public readonly uint Imm871 => (_value >> 1) & 0x7F;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
    }

    readonly struct InstImm6b16w6
    {
        private readonly uint _value;
        public InstImm6b16w6(uint value) => _value = value;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
    }

    readonly struct InstFirstcondb20w4Maskb16w4
    {
        private readonly uint _value;
        public InstFirstcondb20w4Maskb16w4(uint value) => _value = value;
        public readonly uint Mask => (_value >> 16) & 0xF;
        public readonly uint Firstcond => (_value >> 20) & 0xF;
    }

    readonly struct InstCondb28w4Rnb16w4Rtb12w4
    {
        private readonly uint _value;
        public InstCondb28w4Rnb16w4Rtb12w4(uint value) => _value = value;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rtb12w4
    {
        private readonly uint _value;
        public InstRnb16w4Rtb12w4(uint value) => _value = value;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstRnb16w4Rtb12w4Rt2b8w4
    {
        private readonly uint _value;
        public InstRnb16w4Rtb12w4Rt2b8w4(uint value) => _value = value;
        public readonly uint Rt2 => (_value >> 8) & 0xF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Imm8b0w8
    {
        private readonly uint _value;
        public InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstPb24w1Ub23w1Wb21w1Rnb16w4Imm8b0w8
    {
        private readonly uint _value;
        public InstPb24w1Ub23w1Wb21w1Rnb16w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
    }

    readonly struct InstCondb28w4Pb24w1Ub23w1Wb21w1Imm8b0w8
    {
        private readonly uint _value;
        public InstCondb28w4Pb24w1Ub23w1Wb21w1Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstPb24w1Ub23w1Wb21w1Imm8b0w8
    {
        private readonly uint _value;
        public InstPb24w1Ub23w1Wb21w1Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
    }

    readonly struct InstCondb28w4Wb21w1Rnb16w4RegisterListb0w16
    {
        private readonly uint _value;
        public InstCondb28w4Wb21w1Rnb16w4RegisterListb0w16(uint value) => _value = value;
        public readonly uint RegisterList => (_value >> 0) & 0xFFFF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb24w3RegisterListb16w8
    {
        private readonly uint _value;
        public InstRnb24w3RegisterListb16w8(uint value) => _value = value;
        public readonly uint RegisterList => (_value >> 16) & 0xFF;
        public readonly uint Rn => (_value >> 24) & 0x7;
    }

    readonly struct InstWb21w1Rnb16w4Pb15w1Mb14w1RegisterListb0w14
    {
        private readonly uint _value;
        public InstWb21w1Rnb16w4Pb15w1Mb14w1RegisterListb0w14(uint value) => _value = value;
        public readonly uint RegisterList => (_value >> 0) & 0x3FFF;
        public readonly uint M => (_value >> 14) & 0x1;
        public readonly uint P => (_value >> 15) & 0x1;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
    }

    readonly struct InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm12b0w12
    {
        private readonly uint _value;
        public InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm5b7w5Stypeb5w2Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm5b7w5Stypeb5w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 5) & 0x3;
        public readonly uint Imm5 => (_value >> 7) & 0x1F;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rtb12w4Imm8b0w8
    {
        private readonly uint _value;
        public InstRnb16w4Rtb12w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm12b0w12
    {
        private readonly uint _value;
        public InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstImm5b22w5Rnb19w3Rtb16w3
    {
        private readonly uint _value;
        public InstImm5b22w5Rnb19w3Rtb16w3(uint value) => _value = value;
        public readonly uint Rt => (_value >> 16) & 0x7;
        public readonly uint Rn => (_value >> 19) & 0x7;
        public readonly uint Imm5 => (_value >> 22) & 0x1F;
    }

    readonly struct InstRnb16w4Rtb12w4Imm12b0w12
    {
        private readonly uint _value;
        public InstRnb16w4Rtb12w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstRnb16w4Rtb12w4Pb10w1Ub9w1Wb8w1Imm8b0w8
    {
        private readonly uint _value;
        public InstRnb16w4Rtb12w4Pb10w1Ub9w1Wb8w1Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint W => (_value >> 8) & 0x1;
        public readonly uint U => (_value >> 9) & 0x1;
        public readonly uint P => (_value >> 10) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Pb24w1Ub23w1Wb21w1Rtb12w4Imm12b0w12
    {
        private readonly uint _value;
        public InstCondb28w4Pb24w1Ub23w1Wb21w1Rtb12w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstUb23w1Rtb12w4Imm12b0w12
    {
        private readonly uint _value;
        public InstUb23w1Rtb12w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint U => (_value >> 23) & 0x1;
    }

    readonly struct InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm5b7w5Stypeb5w2Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm5b7w5Stypeb5w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 5) & 0x3;
        public readonly uint Imm5 => (_value >> 7) & 0x1F;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRmb22w3Rnb19w3Rtb16w3
    {
        private readonly uint _value;
        public InstRmb22w3Rnb19w3Rtb16w3(uint value) => _value = value;
        public readonly uint Rt => (_value >> 16) & 0x7;
        public readonly uint Rn => (_value >> 19) & 0x7;
        public readonly uint Rm => (_value >> 22) & 0x7;
    }

    readonly struct InstRnb16w4Rtb12w4Imm2b4w2Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rtb12w4Imm2b4w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Imm2 => (_value >> 4) & 0x3;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4(uint value) => _value = value;
        public readonly uint Imm4l => (_value >> 0) & 0xF;
        public readonly uint Imm4h => (_value >> 8) & 0xF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstPb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Rt2b8w4Imm8b0w8
    {
        private readonly uint _value;
        public InstPb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Rt2b8w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rt2 => (_value >> 8) & 0xF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
    }

    readonly struct InstCondb28w4Ub23w1Rtb12w4Imm4hb8w4Imm4lb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Ub23w1Rtb12w4Imm4hb8w4Imm4lb0w4(uint value) => _value = value;
        public readonly uint Imm4l => (_value >> 0) & 0xF;
        public readonly uint Imm4h => (_value >> 8) & 0xF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstPb24w1Ub23w1Wb21w1Rtb12w4Rt2b8w4Imm8b0w8
    {
        private readonly uint _value;
        public InstPb24w1Ub23w1Wb21w1Rtb12w4Rt2b8w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rt2 => (_value >> 8) & 0xF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
    }

    readonly struct InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4(uint value) => _value = value;
        public readonly uint Imm4l => (_value >> 0) & 0xF;
        public readonly uint Imm4h => (_value >> 8) & 0xF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstCondb28w4Ub23w1Rnb16w4Rtb12w4Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Ub23w1Rnb16w4Rtb12w4Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstCondb28w4Pb24w1Ub23w1Wb21w1Rtb12w4Imm4hb8w4Imm4lb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Pb24w1Ub23w1Wb21w1Rtb12w4Imm4hb8w4Imm4lb0w4(uint value) => _value = value;
        public readonly uint Imm4l => (_value >> 0) & 0xF;
        public readonly uint Imm4h => (_value >> 8) & 0xF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRtb24w3Imm8b16w8
    {
        private readonly uint _value;
        public InstRtb24w3Imm8b16w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 16) & 0xFF;
        public readonly uint Rt => (_value >> 24) & 0x7;
    }

    readonly struct InstCondb28w4Opc1b21w3Crnb16w4Rtb12w4Coproc0b8w1Opc2b5w3Crmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Opc1b21w3Crnb16w4Rtb12w4Coproc0b8w1Opc2b5w3Crmb0w4(uint value) => _value = value;
        public readonly uint Crm => (_value >> 0) & 0xF;
        public readonly uint Opc2 => (_value >> 5) & 0x7;
        public readonly uint Coproc0 => (_value >> 8) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Crn => (_value >> 16) & 0xF;
        public readonly uint Opc1 => (_value >> 21) & 0x7;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstOpc1b21w3Crnb16w4Rtb12w4Coproc0b8w1Opc2b5w3Crmb0w4
    {
        private readonly uint _value;
        public InstOpc1b21w3Crnb16w4Rtb12w4Coproc0b8w1Opc2b5w3Crmb0w4(uint value) => _value = value;
        public readonly uint Crm => (_value >> 0) & 0xF;
        public readonly uint Opc2 => (_value >> 5) & 0x7;
        public readonly uint Coproc0 => (_value >> 8) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Crn => (_value >> 16) & 0xF;
        public readonly uint Opc1 => (_value >> 21) & 0x7;
    }

    readonly struct InstCondb28w4Rt2b16w4Rtb12w4Coproc0b8w1Opc1b4w4Crmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rt2b16w4Rtb12w4Coproc0b8w1Opc1b4w4Crmb0w4(uint value) => _value = value;
        public readonly uint Crm => (_value >> 0) & 0xF;
        public readonly uint Opc1 => (_value >> 4) & 0xF;
        public readonly uint Coproc0 => (_value >> 8) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rt2 => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRt2b16w4Rtb12w4Coproc0b8w1Opc1b4w4Crmb0w4
    {
        private readonly uint _value;
        public InstRt2b16w4Rtb12w4Coproc0b8w1Opc1b4w4Crmb0w4(uint value) => _value = value;
        public readonly uint Crm => (_value >> 0) & 0xF;
        public readonly uint Opc1 => (_value >> 4) & 0xF;
        public readonly uint Coproc0 => (_value >> 8) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rt2 => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Sb20w1Rdb16w4Rab12w4Rmb8w4Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Sb20w1Rdb16w4Rab12w4Rmb8w4Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Ra => (_value >> 12) & 0xF;
        public readonly uint Rd => (_value >> 16) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rab12w4Rdb8w4Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rab12w4Rdb8w4Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Ra => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rdb16w4Rab12w4Rmb8w4Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdb16w4Rab12w4Rmb8w4Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Ra => (_value >> 12) & 0xF;
        public readonly uint Rd => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstCondb28w4Imm4b16w4Rdb12w4Imm12b0w12
    {
        private readonly uint _value;
        public InstCondb28w4Imm4b16w4Rdb12w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Imm4 => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstIb26w1Imm4b16w4Imm3b12w3Rdb8w4Imm8b0w8
    {
        private readonly uint _value;
        public InstIb26w1Imm4b16w4Imm3b12w3Rdb8w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint Imm4 => (_value >> 16) & 0xF;
        public readonly uint I => (_value >> 26) & 0x1;
    }

    readonly struct InstDb23w1Rmb19w4Rdb16w3
    {
        private readonly uint _value;
        public InstDb23w1Rmb19w4Rdb16w3(uint value) => _value = value;
        public readonly uint Rd => (_value >> 16) & 0x7;
        public readonly uint Rm => (_value >> 19) & 0xF;
        public readonly uint D => (_value >> 23) & 0x1;
    }

    readonly struct InstOpb27w2Imm5b22w5Rmb19w3Rdb16w3
    {
        private readonly uint _value;
        public InstOpb27w2Imm5b22w5Rmb19w3Rdb16w3(uint value) => _value = value;
        public readonly uint Rd => (_value >> 16) & 0x7;
        public readonly uint Rm => (_value >> 19) & 0x7;
        public readonly uint Imm5 => (_value >> 22) & 0x1F;
        public readonly uint Op => (_value >> 27) & 0x3;
    }

    readonly struct InstCondb28w4Sb20w1Rdb12w4Rsb8w4Stypeb5w2Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Sb20w1Rdb12w4Rsb8w4Stypeb5w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 5) & 0x3;
        public readonly uint Rs => (_value >> 8) & 0xF;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRsb19w3Rdmb16w3
    {
        private readonly uint _value;
        public InstRsb19w3Rdmb16w3(uint value) => _value = value;
        public readonly uint Rdm => (_value >> 16) & 0x7;
        public readonly uint Rs => (_value >> 19) & 0x7;
    }

    readonly struct InstStypeb21w2Sb20w1Rmb16w4Rdb8w4Rsb0w4
    {
        private readonly uint _value;
        public InstStypeb21w2Sb20w1Rmb16w4Rdb8w4Rsb0w4(uint value) => _value = value;
        public readonly uint Rs => (_value >> 0) & 0xF;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Rm => (_value >> 16) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint Stype => (_value >> 21) & 0x3;
    }

    readonly struct InstCondb28w4Rb22w1Rdb12w4
    {
        private readonly uint _value;
        public InstCondb28w4Rb22w1Rdb12w4(uint value) => _value = value;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint R => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRb20w1Rdb8w4
    {
        private readonly uint _value;
        public InstRb20w1Rdb8w4(uint value) => _value = value;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint R => (_value >> 20) & 0x1;
    }

    readonly struct InstCondb28w4Rb22w1M1b16w4Rdb12w4Mb8w1
    {
        private readonly uint _value;
        public InstCondb28w4Rb22w1M1b16w4Rdb12w4Mb8w1(uint value) => _value = value;
        public readonly uint M => (_value >> 8) & 0x1;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint M1 => (_value >> 16) & 0xF;
        public readonly uint R => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRb20w1M1b16w4Rdb8w4Mb4w1
    {
        private readonly uint _value;
        public InstRb20w1M1b16w4Rdb8w4Mb4w1(uint value) => _value = value;
        public readonly uint M => (_value >> 4) & 0x1;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint M1 => (_value >> 16) & 0xF;
        public readonly uint R => (_value >> 20) & 0x1;
    }

    readonly struct InstCondb28w4Rb22w1M1b16w4Mb8w1Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rb22w1M1b16w4Mb8w1Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 8) & 0x1;
        public readonly uint M1 => (_value >> 16) & 0xF;
        public readonly uint R => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRb20w1Rnb16w4M1b8w4Mb4w1
    {
        private readonly uint _value;
        public InstRb20w1Rnb16w4M1b8w4Mb4w1(uint value) => _value = value;
        public readonly uint M => (_value >> 4) & 0x1;
        public readonly uint M1 => (_value >> 8) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint R => (_value >> 20) & 0x1;
    }

    readonly struct InstCondb28w4Rb22w1Maskb16w4Imm12b0w12
    {
        private readonly uint _value;
        public InstCondb28w4Rb22w1Maskb16w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Mask => (_value >> 16) & 0xF;
        public readonly uint R => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstCondb28w4Rb22w1Maskb16w4Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rb22w1Maskb16w4Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint Mask => (_value >> 16) & 0xF;
        public readonly uint R => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRb20w1Rnb16w4Maskb8w4
    {
        private readonly uint _value;
        public InstRb20w1Rnb16w4Maskb8w4(uint value) => _value = value;
        public readonly uint Mask => (_value >> 8) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint R => (_value >> 20) & 0x1;
    }

    readonly struct InstCondb28w4Sb20w1Rdb16w4Rmb8w4Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Sb20w1Rdb16w4Rmb8w4Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Rd => (_value >> 16) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb19w3Rdmb16w3
    {
        private readonly uint _value;
        public InstRnb19w3Rdmb16w3(uint value) => _value = value;
        public readonly uint Rdm => (_value >> 16) & 0x7;
        public readonly uint Rn => (_value >> 19) & 0x7;
    }

    readonly struct InstRmb19w3Rdb16w3
    {
        private readonly uint _value;
        public InstRmb19w3Rdb16w3(uint value) => _value = value;
        public readonly uint Rd => (_value >> 16) & 0x7;
        public readonly uint Rm => (_value >> 19) & 0x7;
    }

    readonly struct InstCondb28w4Rnb16w4Rdb12w4Imm5b7w5Tbb6w1Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rnb16w4Rdb12w4Imm5b7w5Tbb6w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Tb => (_value >> 6) & 0x1;
        public readonly uint Imm5 => (_value >> 7) & 0x1F;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Imm3b12w3Rdb8w4Imm2b6w2Tbb5w1Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Imm3b12w3Rdb8w4Imm2b6w2Tbb5w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Tb => (_value >> 5) & 0x1;
        public readonly uint Imm2 => (_value >> 6) & 0x3;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstUb23w1Rb22w1Rnb16w4Imm12b0w12
    {
        private readonly uint _value;
        public InstUb23w1Rb22w1Rnb16w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint R => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
    }

    readonly struct InstWb21w1Rnb16w4Imm12b0w12
    {
        private readonly uint _value;
        public InstWb21w1Rnb16w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
    }

    readonly struct InstWb21w1Rnb16w4Imm8b0w8
    {
        private readonly uint _value;
        public InstWb21w1Rnb16w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
    }

    readonly struct InstUb23w1Imm12b0w12
    {
        private readonly uint _value;
        public InstUb23w1Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint U => (_value >> 23) & 0x1;
    }

    readonly struct InstUb23w1Rb22w1Rnb16w4Imm5b7w5Stypeb5w2Rmb0w4
    {
        private readonly uint _value;
        public InstUb23w1Rb22w1Rnb16w4Imm5b7w5Stypeb5w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 5) & 0x3;
        public readonly uint Imm5 => (_value >> 7) & 0x1F;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint R => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
    }

    readonly struct InstWb21w1Rnb16w4Imm2b4w2Rmb0w4
    {
        private readonly uint _value;
        public InstWb21w1Rnb16w4Imm2b4w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Imm2 => (_value >> 4) & 0x3;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
    }

    readonly struct InstUb23w1Rnb16w4Imm12b0w12
    {
        private readonly uint _value;
        public InstUb23w1Rnb16w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint U => (_value >> 23) & 0x1;
    }

    readonly struct InstRnb16w4Imm12b0w12
    {
        private readonly uint _value;
        public InstRnb16w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstRnb16w4Imm8b0w8
    {
        private readonly uint _value;
        public InstRnb16w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstUb23w1Rnb16w4Imm5b7w5Stypeb5w2Rmb0w4
    {
        private readonly uint _value;
        public InstUb23w1Rnb16w4Imm5b7w5Stypeb5w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Stype => (_value >> 5) & 0x3;
        public readonly uint Imm5 => (_value >> 7) & 0x1F;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint U => (_value >> 23) & 0x1;
    }

    readonly struct InstRnb16w4Imm2b4w2Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Imm2b4w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Imm2 => (_value >> 4) & 0x3;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstPb24w1RegisterListb16w8
    {
        private readonly uint _value;
        public InstPb24w1RegisterListb16w8(uint value) => _value = value;
        public readonly uint RegisterList => (_value >> 16) & 0xFF;
        public readonly uint P => (_value >> 24) & 0x1;
    }

    readonly struct InstMb24w1RegisterListb16w8
    {
        private readonly uint _value;
        public InstMb24w1RegisterListb16w8(uint value) => _value = value;
        public readonly uint RegisterList => (_value >> 16) & 0xFF;
        public readonly uint M => (_value >> 24) & 0x1;
    }

    readonly struct InstCondb28w4Rnb16w4Rdb12w4Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rnb16w4Rdb12w4Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb19w3Rdb16w3
    {
        private readonly uint _value;
        public InstRnb19w3Rdb16w3(uint value) => _value = value;
        public readonly uint Rd => (_value >> 16) & 0x7;
        public readonly uint Rn => (_value >> 19) & 0x7;
    }

    readonly struct InstCondb28w4Widthm1b16w5Rdb12w4Lsbb7w5Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Widthm1b16w5Rdb12w4Lsbb7w5Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint Lsb => (_value >> 7) & 0x1F;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Widthm1 => (_value >> 16) & 0x1F;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Imm3b12w3Rdb8w4Imm2b6w2Widthm1b0w5
    {
        private readonly uint _value;
        public InstRnb16w4Imm3b12w3Rdb8w4Imm2b6w2Widthm1b0w5(uint value) => _value = value;
        public readonly uint Widthm1 => (_value >> 0) & 0x1F;
        public readonly uint Imm2 => (_value >> 6) & 0x3;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rdb16w4Rmb8w4Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdb16w4Rmb8w4Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Rd => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstEb9w1
    {
        private readonly uint _value;
        public InstEb9w1(uint value) => _value = value;
        public readonly uint E => (_value >> 9) & 0x1;
    }

    readonly struct InstEb19w1
    {
        private readonly uint _value;
        public InstEb19w1(uint value) => _value = value;
        public readonly uint E => (_value >> 19) & 0x1;
    }

    readonly struct InstImm1b9w1
    {
        private readonly uint _value;
        public InstImm1b9w1(uint value) => _value = value;
        public readonly uint Imm1 => (_value >> 9) & 0x1;
    }

    readonly struct InstImm1b19w1
    {
        private readonly uint _value;
        public InstImm1b19w1(uint value) => _value = value;
        public readonly uint Imm1 => (_value >> 19) & 0x1;
    }

    readonly struct InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstCondb28w4Rdb16w4Rab12w4Rmb8w4Mb6w1Nb5w1Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdb16w4Rab12w4Rmb8w4Mb6w1Nb5w1Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint N => (_value >> 5) & 0x1;
        public readonly uint M => (_value >> 6) & 0x1;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Ra => (_value >> 12) & 0xF;
        public readonly uint Rd => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rab12w4Rdb8w4Nb5w1Mb4w1Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rab12w4Rdb8w4Nb5w1Mb4w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 4) & 0x1;
        public readonly uint N => (_value >> 5) & 0x1;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Ra => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rdb16w4Rab12w4Rmb8w4Mb5w1Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdb16w4Rab12w4Rmb8w4Mb5w1Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Ra => (_value >> 12) & 0xF;
        public readonly uint Rd => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rab12w4Rdb8w4Mb4w1Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rab12w4Rdb8w4Mb4w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 4) & 0x1;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Ra => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Sb20w1Rdhib16w4Rdlob12w4Rmb8w4Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Sb20w1Rdhib16w4Rdlob12w4Rmb8w4Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Rdlo => (_value >> 12) & 0xF;
        public readonly uint Rdhi => (_value >> 16) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rdlob12w4Rdhib8w4Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rdlob12w4Rdhib8w4Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Rdhi => (_value >> 8) & 0xF;
        public readonly uint Rdlo => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rdhib16w4Rdlob12w4Rmb8w4Mb6w1Nb5w1Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdhib16w4Rdlob12w4Rmb8w4Mb6w1Nb5w1Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint N => (_value >> 5) & 0x1;
        public readonly uint M => (_value >> 6) & 0x1;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Rdlo => (_value >> 12) & 0xF;
        public readonly uint Rdhi => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rdlob12w4Rdhib8w4Nb5w1Mb4w1Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rdlob12w4Rdhib8w4Nb5w1Mb4w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 4) & 0x1;
        public readonly uint N => (_value >> 5) & 0x1;
        public readonly uint Rdhi => (_value >> 8) & 0xF;
        public readonly uint Rdlo => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rdhib16w4Rdlob12w4Rmb8w4Mb5w1Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdhib16w4Rdlob12w4Rmb8w4Mb5w1Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Rdlo => (_value >> 12) & 0xF;
        public readonly uint Rdhi => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rdlob12w4Rdhib8w4Mb4w1Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rdlob12w4Rdhib8w4Mb4w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 4) & 0x1;
        public readonly uint Rdhi => (_value >> 8) & 0xF;
        public readonly uint Rdlo => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rdb16w4Rab12w4Rmb8w4Mb6w1Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdb16w4Rab12w4Rmb8w4Mb6w1Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 6) & 0x1;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Ra => (_value >> 12) & 0xF;
        public readonly uint Rd => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstCondb28w4Rdb16w4Rab12w4Rmb8w4Rb5w1Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdb16w4Rab12w4Rmb8w4Rb5w1Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint R => (_value >> 5) & 0x1;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Ra => (_value >> 12) & 0xF;
        public readonly uint Rd => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rab12w4Rdb8w4Rb4w1Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rab12w4Rdb8w4Rb4w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint R => (_value >> 4) & 0x1;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Ra => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rdb16w4Rmb8w4Rb5w1Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdb16w4Rmb8w4Rb5w1Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint R => (_value >> 5) & 0x1;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Rd => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rdb8w4Rb4w1Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rdb8w4Rb4w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint R => (_value >> 4) & 0x1;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rdb16w4Rmb8w4Mb5w1Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdb16w4Rmb8w4Mb5w1Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Rd => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rdb8w4Mb4w1Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rdb8w4Mb4w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 4) & 0x1;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rdb16w4Rmb8w4Mb6w1Nb5w1Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdb16w4Rmb8w4Mb6w1Nb5w1Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint N => (_value >> 5) & 0x1;
        public readonly uint M => (_value >> 6) & 0x1;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Rd => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rdb8w4Nb5w1Mb4w1Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rdb8w4Nb5w1Mb4w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 4) & 0x1;
        public readonly uint N => (_value >> 5) & 0x1;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rdb16w4Rmb8w4Mb6w1Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdb16w4Rmb8w4Mb6w1Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 6) & 0x1;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Rd => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstCondb28w4SatImmb16w5Rdb12w4Imm5b7w5Shb6w1Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4SatImmb16w5Rdb12w4Imm5b7w5Shb6w1Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint Sh => (_value >> 6) & 0x1;
        public readonly uint Imm5 => (_value >> 7) & 0x1F;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint SatImm => (_value >> 16) & 0x1F;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstShb21w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2SatImmb0w5
    {
        private readonly uint _value;
        public InstShb21w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2SatImmb0w5(uint value) => _value = value;
        public readonly uint SatImm => (_value >> 0) & 0x1F;
        public readonly uint Imm2 => (_value >> 6) & 0x3;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Imm3 => (_value >> 12) & 0x7;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint Sh => (_value >> 21) & 0x1;
    }

    readonly struct InstCondb28w4SatImmb16w4Rdb12w4Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4SatImmb16w4Rdb12w4Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint SatImm => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rdb8w4SatImmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rdb8w4SatImmb0w4(uint value) => _value = value;
        public readonly uint SatImm => (_value >> 0) & 0xF;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rnb16w4Rtb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rnb16w4Rtb0w4(uint value) => _value = value;
        public readonly uint Rt => (_value >> 0) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstCondb28w4Rnb16w4Rdb12w4Rtb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rnb16w4Rdb12w4Rtb0w4(uint value) => _value = value;
        public readonly uint Rt => (_value >> 0) & 0xF;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rtb12w4Rdb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rtb12w4Rdb0w4(uint value) => _value = value;
        public readonly uint Rd => (_value >> 0) & 0xF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstRnb16w4Rtb12w4Rt2b8w4Rdb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rtb12w4Rt2b8w4Rdb0w4(uint value) => _value = value;
        public readonly uint Rd => (_value >> 0) & 0xF;
        public readonly uint Rt2 => (_value >> 8) & 0xF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstWb21w1Rnb16w4Mb14w1RegisterListb0w14
    {
        private readonly uint _value;
        public InstWb21w1Rnb16w4Mb14w1RegisterListb0w14(uint value) => _value = value;
        public readonly uint RegisterList => (_value >> 0) & 0x3FFF;
        public readonly uint M => (_value >> 14) & 0x1;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
    }

    readonly struct InstRnb16w4Rtb12w4Rdb8w4Imm8b0w8
    {
        private readonly uint _value;
        public InstRnb16w4Rtb12w4Rdb8w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rnb16w4Rdb12w4Rotateb10w2Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rnb16w4Rdb12w4Rotateb10w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Rotate => (_value >> 10) & 0x3;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRnb16w4Rdb8w4Rotateb4w2Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Rdb8w4Rotateb4w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Rotate => (_value >> 4) & 0x3;
        public readonly uint Rd => (_value >> 8) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rdb12w4Rotateb10w2Rmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdb12w4Rotateb10w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Rotate => (_value >> 10) & 0x3;
        public readonly uint Rd => (_value >> 12) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRdb8w4Rotateb4w2Rmb0w4
    {
        private readonly uint _value;
        public InstRdb8w4Rotateb4w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Rotate => (_value >> 4) & 0x3;
        public readonly uint Rd => (_value >> 8) & 0xF;
    }

    readonly struct InstRnb16w4Hb4w1Rmb0w4
    {
        private readonly uint _value;
        public InstRnb16w4Hb4w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint H => (_value >> 4) & 0x1;
        public readonly uint Rn => (_value >> 16) & 0xF;
    }

    readonly struct InstImm12b8w12Imm4b0w4
    {
        private readonly uint _value;
        public InstImm12b8w12Imm4b0w4(uint value) => _value = value;
        public readonly uint Imm4 => (_value >> 0) & 0xF;
        public readonly uint Imm12 => (_value >> 8) & 0xFFF;
    }

    readonly struct InstImm4b16w4Imm12b0w12
    {
        private readonly uint _value;
        public InstImm4b16w4Imm12b0w12(uint value) => _value = value;
        public readonly uint Imm12 => (_value >> 0) & 0xFFF;
        public readonly uint Imm4 => (_value >> 16) & 0xF;
    }

    readonly struct InstCondb28w4Rdhib16w4Rdlob12w4Rmb8w4Rnb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Rdhib16w4Rdlob12w4Rmb8w4Rnb0w4(uint value) => _value = value;
        public readonly uint Rn => (_value >> 0) & 0xF;
        public readonly uint Rm => (_value >> 8) & 0xF;
        public readonly uint Rdlo => (_value >> 12) & 0xF;
        public readonly uint Rdhi => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 24) & 0x1;
    }

    readonly struct InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 28) & 0x1;
    }

    readonly struct InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 24) & 0x1;
    }

    readonly struct InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 28) & 0x1;
    }

    readonly struct InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Sz => (_value >> 20) & 0x1;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint F => (_value >> 10) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Size => (_value >> 18) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstCondb28w4Db22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Db22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4
    {
        private readonly uint _value;
        public InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4(uint value) => _value = value;
        public readonly uint Imm4 => (_value >> 0) & 0xF;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm3 => (_value >> 16) & 0x7;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint I => (_value >> 24) & 0x1;
    }

    readonly struct InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4
    {
        private readonly uint _value;
        public InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4(uint value) => _value = value;
        public readonly uint Imm4 => (_value >> 0) & 0xF;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm3 => (_value >> 16) & 0x7;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint I => (_value >> 28) & 0x1;
    }

    readonly struct InstRotb24w1Db22w1Sb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstRotb24w1Db22w1Sb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Rot => (_value >> 24) & 0x1;
    }

    readonly struct InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Size => (_value >> 18) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstRotb23w2Db22w1Sb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstRotb23w2Db22w1Sb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint S => (_value >> 20) & 0x1;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Rot => (_value >> 23) & 0x3;
    }

    readonly struct InstSb23w1Db22w1Rotb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstSb23w1Db22w1Rotb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Rot => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint S => (_value >> 23) & 0x1;
    }

    readonly struct InstCondb28w4Db22w1Vdb12w4Sizeb8w2
    {
        private readonly uint _value;
        public InstCondb28w4Db22w1Vdb12w4Sizeb8w2(uint value) => _value = value;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstDb22w1Vdb12w4Sizeb8w2
    {
        private readonly uint _value;
        public InstDb22w1Vdb12w4Sizeb8w2(uint value) => _value = value;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint Op => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Size => (_value >> 18) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Op => (_value >> 7) & 0x1;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstCondb28w4Db22w1Opb16w1Vdb12w4Szb8w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Db22w1Opb16w1Vdb12w4Szb8w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Sz => (_value >> 8) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Op => (_value >> 16) & 0x1;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstDb22w1Opb16w1Vdb12w4Szb8w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Opb16w1Vdb12w4Szb8w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Sz => (_value >> 8) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Op => (_value >> 16) & 0x1;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstCondb28w4Db22w1Vdb12w4Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Db22w1Vdb12w4Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstDb22w1Vdb12w4Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Vdb12w4Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Sizeb18w2Vdb12w4Opb8w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Sizeb18w2Vdb12w4Opb8w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Op => (_value >> 8) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Size => (_value >> 18) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Sizeb18w2Vdb12w4Opb7w2Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Sizeb18w2Vdb12w4Opb7w2Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint Op => (_value >> 7) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Size => (_value >> 18) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstCondb28w4Db22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Db22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Op => (_value >> 7) & 0x1;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstUb24w1Db22w1Imm6b16w6Vdb12w4Opb8w2Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb24w1Db22w1Imm6b16w6Vdb12w4Opb8w2Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint Op => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 24) & 0x1;
    }

    readonly struct InstUb28w1Db22w1Imm6b16w6Vdb12w4Opb8w2Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb28w1Db22w1Imm6b16w6Vdb12w4Opb8w2Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint Op => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 28) & 0x1;
    }

    readonly struct InstCondb28w4Db22w1Opb18w1Ub16w1Vdb12w4Sfb8w2Sxb7w1Ib5w1Imm4b0w4
    {
        private readonly uint _value;
        public InstCondb28w4Db22w1Opb18w1Ub16w1Vdb12w4Sfb8w2Sxb7w1Ib5w1Imm4b0w4(uint value) => _value = value;
        public readonly uint Imm4 => (_value >> 0) & 0xF;
        public readonly uint I => (_value >> 5) & 0x1;
        public readonly uint Sx => (_value >> 7) & 0x1;
        public readonly uint Sf => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint U => (_value >> 16) & 0x1;
        public readonly uint Op => (_value >> 18) & 0x1;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstDb22w1Opb18w1Ub16w1Vdb12w4Sfb8w2Sxb7w1Ib5w1Imm4b0w4
    {
        private readonly uint _value;
        public InstDb22w1Opb18w1Ub16w1Vdb12w4Sfb8w2Sxb7w1Ib5w1Imm4b0w4(uint value) => _value = value;
        public readonly uint Imm4 => (_value >> 0) & 0xF;
        public readonly uint I => (_value >> 5) & 0x1;
        public readonly uint Sx => (_value >> 7) & 0x1;
        public readonly uint Sf => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint U => (_value >> 16) & 0x1;
        public readonly uint Op => (_value >> 18) & 0x1;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstCondb28w4Bb22w1Qb21w1Vdb16w4Rtb12w4Db7w1Eb5w1
    {
        private readonly uint _value;
        public InstCondb28w4Bb22w1Qb21w1Vdb16w4Rtb12w4Db7w1Eb5w1(uint value) => _value = value;
        public readonly uint E => (_value >> 5) & 0x1;
        public readonly uint D => (_value >> 7) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Vd => (_value >> 16) & 0xF;
        public readonly uint Q => (_value >> 21) & 0x1;
        public readonly uint B => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstBb22w1Qb21w1Vdb16w4Rtb12w4Db7w1Eb5w1
    {
        private readonly uint _value;
        public InstBb22w1Qb21w1Vdb16w4Rtb12w4Db7w1Eb5w1(uint value) => _value = value;
        public readonly uint E => (_value >> 5) & 0x1;
        public readonly uint D => (_value >> 7) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Vd => (_value >> 16) & 0xF;
        public readonly uint Q => (_value >> 21) & 0x1;
        public readonly uint B => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Imm4b16w4Vdb12w4Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Imm4b16w4Vdb12w4Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm4 => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Vnb16w4Vdb12w4Imm4b8w4Nb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Vnb16w4Vdb12w4Imm4b8w4Nb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Imm4 => (_value >> 8) & 0xF;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint IndexAlign => (_value >> 4) & 0xF;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Tb5w1Ab4w1Rmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Tb5w1Ab4w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint A => (_value >> 4) & 0x1;
        public readonly uint T => (_value >> 5) & 0x1;
        public readonly uint Size => (_value >> 6) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint Align => (_value >> 4) & 0x3;
        public readonly uint Size => (_value >> 6) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Tb5w1Rmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Tb5w1Rmb0w4(uint value) => _value = value;
        public readonly uint Rm => (_value >> 0) & 0xF;
        public readonly uint T => (_value >> 5) & 0x1;
        public readonly uint Size => (_value >> 6) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstCondb28w4Pb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm8b0w8
    {
        private readonly uint _value;
        public InstCondb28w4Pb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstPb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm8b0w8
    {
        private readonly uint _value;
        public InstPb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint W => (_value >> 21) & 0x1;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint P => (_value >> 24) & 0x1;
    }

    readonly struct InstCondb28w4Ub23w1Db22w1Rnb16w4Vdb12w4Sizeb8w2Imm8b0w8
    {
        private readonly uint _value;
        public InstCondb28w4Ub23w1Db22w1Rnb16w4Vdb12w4Sizeb8w2Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstUb23w1Db22w1Rnb16w4Vdb12w4Sizeb8w2Imm8b0w8
    {
        private readonly uint _value;
        public InstUb23w1Db22w1Rnb16w4Vdb12w4Sizeb8w2Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Rn => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
    }

    readonly struct InstCondb28w4Ub23w1Db22w1Vdb12w4Sizeb8w2Imm8b0w8
    {
        private readonly uint _value;
        public InstCondb28w4Ub23w1Db22w1Vdb12w4Sizeb8w2Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstUb23w1Db22w1Vdb12w4Sizeb8w2Imm8b0w8
    {
        private readonly uint _value;
        public InstUb23w1Db22w1Vdb12w4Sizeb8w2Imm8b0w8(uint value) => _value = value;
        public readonly uint Imm8 => (_value >> 0) & 0xFF;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 23) & 0x1;
    }

    readonly struct InstQb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Fb8w1Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstQb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Fb8w1Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint F => (_value >> 8) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Q => (_value >> 24) & 0x1;
    }

    readonly struct InstQb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Fb8w1Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstQb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Fb8w1Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint F => (_value >> 8) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Q => (_value >> 28) & 0x1;
    }

    readonly struct InstDb22w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstUb24w1Db22w1Imm3hb19w3Vdb12w4Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb24w1Db22w1Imm3hb19w3Vdb12w4Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm3h => (_value >> 19) & 0x7;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 24) & 0x1;
    }

    readonly struct InstUb28w1Db22w1Imm3hb19w3Vdb12w4Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb28w1Db22w1Imm3hb19w3Vdb12w4Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm3h => (_value >> 19) & 0x7;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 28) & 0x1;
    }

    readonly struct InstCondb28w4Opb20w1Rt2b16w4Rtb12w4Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Opb20w1Rt2b16w4Rtb12w4Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rt2 => (_value >> 16) & 0xF;
        public readonly uint Op => (_value >> 20) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstOpb20w1Rt2b16w4Rtb12w4Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstOpb20w1Rt2b16w4Rtb12w4Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Rt2 => (_value >> 16) & 0xF;
        public readonly uint Op => (_value >> 20) & 0x1;
    }

    readonly struct InstCondb28w4Opb20w1Vnb16w4Rtb12w4Nb7w1
    {
        private readonly uint _value;
        public InstCondb28w4Opb20w1Vnb16w4Rtb12w4Nb7w1(uint value) => _value = value;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Op => (_value >> 20) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstOpb20w1Vnb16w4Rtb12w4Nb7w1
    {
        private readonly uint _value;
        public InstOpb20w1Vnb16w4Rtb12w4Nb7w1(uint value) => _value = value;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Op => (_value >> 20) & 0x1;
    }

    readonly struct InstCondb28w4Db22w1Imm4hb16w4Vdb12w4Sizeb8w2Imm4lb0w4
    {
        private readonly uint _value;
        public InstCondb28w4Db22w1Imm4hb16w4Vdb12w4Sizeb8w2Imm4lb0w4(uint value) => _value = value;
        public readonly uint Imm4l => (_value >> 0) & 0xF;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm4h => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstDb22w1Imm4hb16w4Vdb12w4Sizeb8w2Imm4lb0w4
    {
        private readonly uint _value;
        public InstDb22w1Imm4hb16w4Vdb12w4Sizeb8w2Imm4lb0w4(uint value) => _value = value;
        public readonly uint Imm4l => (_value >> 0) & 0xF;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm4h => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstCondb28w4Opc1b21w2Vdb16w4Rtb12w4Db7w1Opc2b5w2
    {
        private readonly uint _value;
        public InstCondb28w4Opc1b21w2Vdb16w4Rtb12w4Db7w1Opc2b5w2(uint value) => _value = value;
        public readonly uint Opc2 => (_value >> 5) & 0x3;
        public readonly uint D => (_value >> 7) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Vd => (_value >> 16) & 0xF;
        public readonly uint Opc1 => (_value >> 21) & 0x3;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstOpc1b21w2Vdb16w4Rtb12w4Db7w1Opc2b5w2
    {
        private readonly uint _value;
        public InstOpc1b21w2Vdb16w4Rtb12w4Db7w1Opc2b5w2(uint value) => _value = value;
        public readonly uint Opc2 => (_value >> 5) & 0x3;
        public readonly uint D => (_value >> 7) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Vd => (_value >> 16) & 0xF;
        public readonly uint Opc1 => (_value >> 21) & 0x3;
    }

    readonly struct InstCondb28w4Ub23w1Opc1b21w2Vnb16w4Rtb12w4Nb7w1Opc2b5w2
    {
        private readonly uint _value;
        public InstCondb28w4Ub23w1Opc1b21w2Vnb16w4Rtb12w4Nb7w1Opc2b5w2(uint value) => _value = value;
        public readonly uint Opc2 => (_value >> 5) & 0x3;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Opc1 => (_value >> 21) & 0x3;
        public readonly uint U => (_value >> 23) & 0x1;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstUb23w1Opc1b21w2Vnb16w4Rtb12w4Nb7w1Opc2b5w2
    {
        private readonly uint _value;
        public InstUb23w1Opc1b21w2Vnb16w4Rtb12w4Nb7w1Opc2b5w2(uint value) => _value = value;
        public readonly uint Opc2 => (_value >> 5) & 0x3;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Opc1 => (_value >> 21) & 0x3;
        public readonly uint U => (_value >> 23) & 0x1;
    }

    readonly struct InstCondb28w4Regb16w4Rtb12w4
    {
        private readonly uint _value;
        public InstCondb28w4Regb16w4Rtb12w4(uint value) => _value = value;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Reg => (_value >> 16) & 0xF;
        public readonly uint Cond => (_value >> 28) & 0xF;
    }

    readonly struct InstRegb16w4Rtb12w4
    {
        private readonly uint _value;
        public InstRegb16w4Rtb12w4(uint value) => _value = value;
        public readonly uint Rt => (_value >> 12) & 0xF;
        public readonly uint Reg => (_value >> 16) & 0xF;
    }

    readonly struct InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Opb9w1Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Opb9w1Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Op => (_value >> 9) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 24) & 0x1;
    }

    readonly struct InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Opb9w1Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Opb9w1Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Op => (_value >> 9) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 28) & 0x1;
    }

    readonly struct InstOpb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstOpb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Op => (_value >> 24) & 0x1;
    }

    readonly struct InstOpb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstOpb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Op => (_value >> 28) & 0x1;
    }

    readonly struct InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Sz => (_value >> 20) & 0x1;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstQb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstQb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Q => (_value >> 24) & 0x1;
    }

    readonly struct InstQb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstQb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Size => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint Q => (_value >> 28) & 0x1;
    }

    readonly struct InstDb22w1Sizeb18w2Vdb12w4Opb6w2Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Sizeb18w2Vdb12w4Opb6w2Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Op => (_value >> 6) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Size => (_value >> 18) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstUb24w1Db22w1Imm6b16w6Vdb12w4Opb8w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb24w1Db22w1Imm6b16w6Vdb12w4Opb8w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Op => (_value >> 8) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 24) & 0x1;
    }

    readonly struct InstUb28w1Db22w1Imm6b16w6Vdb12w4Opb8w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb28w1Db22w1Imm6b16w6Vdb12w4Opb8w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Op => (_value >> 8) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 28) & 0x1;
    }

    readonly struct InstUb24w1Db22w1Imm6b16w6Vdb12w4Opb8w1Lb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb24w1Db22w1Imm6b16w6Vdb12w4Opb8w1Lb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint L => (_value >> 7) & 0x1;
        public readonly uint Op => (_value >> 8) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 24) & 0x1;
    }

    readonly struct InstUb28w1Db22w1Imm6b16w6Vdb12w4Opb8w1Lb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb28w1Db22w1Imm6b16w6Vdb12w4Opb8w1Lb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint L => (_value >> 7) & 0x1;
        public readonly uint Op => (_value >> 8) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 28) & 0x1;
    }

    readonly struct InstDb22w1Sizeb18w2Vdb12w4Fb8w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Sizeb18w2Vdb12w4Fb8w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint F => (_value >> 8) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Size => (_value >> 18) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstUb24w1Db22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb24w1Db22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint L => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 24) & 0x1;
    }

    readonly struct InstUb28w1Db22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb28w1Db22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint L => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 28) & 0x1;
    }

    readonly struct InstDb22w1Imm6b16w6Vdb12w4Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Imm6b16w6Vdb12w4Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Ccb20w2Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Ccb20w2Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Size => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint Cc => (_value >> 20) & 0x3;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstUb24w1Db22w1Imm6b16w6Vdb12w4Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb24w1Db22w1Imm6b16w6Vdb12w4Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 24) & 0x1;
    }

    readonly struct InstUb28w1Db22w1Imm6b16w6Vdb12w4Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstUb28w1Db22w1Imm6b16w6Vdb12w4Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint D => (_value >> 22) & 0x1;
        public readonly uint U => (_value >> 28) & 0x1;
    }

    readonly struct InstDb22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint L => (_value >> 7) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Imm6 => (_value >> 16) & 0x3F;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Vdb12w4Qb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Vdb12w4Qb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Q => (_value >> 6) & 0x1;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }

    readonly struct InstDb22w1Vnb16w4Vdb12w4Lenb8w2Nb7w1Opb6w1Mb5w1Vmb0w4
    {
        private readonly uint _value;
        public InstDb22w1Vnb16w4Vdb12w4Lenb8w2Nb7w1Opb6w1Mb5w1Vmb0w4(uint value) => _value = value;
        public readonly uint Vm => (_value >> 0) & 0xF;
        public readonly uint M => (_value >> 5) & 0x1;
        public readonly uint Op => (_value >> 6) & 0x1;
        public readonly uint N => (_value >> 7) & 0x1;
        public readonly uint Len => (_value >> 8) & 0x3;
        public readonly uint Vd => (_value >> 12) & 0xF;
        public readonly uint Vn => (_value >> 16) & 0xF;
        public readonly uint D => (_value >> 22) & 0x1;
    }
}

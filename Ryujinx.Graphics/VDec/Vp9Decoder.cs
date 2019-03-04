using System.Collections.Generic;
using System.IO;

namespace Ryujinx.Graphics.VDec
{
    class Vp9Decoder
    {
        private const int DiffUpdateProbability = 252;

        private const int FrameSyncCode = 0x498342;

        private static readonly int[] MapLut = new int[]
        {
            20,  21,  22,  23,  24,  25,  0,   26,  27,  28,  29,  30,  31,  32,  33,  34,
            35,  36,  37,  1,   38,  39,  40,  41,  42,  43,  44,  45,  46,  47,  48,  49,
            2,   50,  51,  52,  53,  54,  55,  56,  57,  58,  59,  60,  61,  3,   62,  63,
            64,  65,  66,  67,  68,  69,  70,  71,  72,  73,  4,   74,  75,  76,  77,  78,
            79,  80,  81,  82,  83,  84,  85,  5,   86,  87,  88,  89,  90,  91,  92,  93,
            94,  95,  96,  97,  6,   98,  99,  100, 101, 102, 103, 104, 105, 106, 107, 108,
            109, 7,   110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 8,   122,
            123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 9,   134, 135, 136, 137,
            138, 139, 140, 141, 142, 143, 144, 145, 10,  146, 147, 148, 149, 150, 151, 152,
            153, 154, 155, 156, 157, 11,  158, 159, 160, 161, 162, 163, 164, 165, 166, 167,
            168, 169, 12,  170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 13,
            182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 14,  194, 195, 196,
            197, 198, 199, 200, 201, 202, 203, 204, 205, 15,  206, 207, 208, 209, 210, 211,
            212, 213, 214, 215, 216, 217, 16,  218, 219, 220, 221, 222, 223, 224, 225, 226,
            227, 228, 229, 17,  230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241,
            18,  242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 19
        };

        private byte[] DefaultTx8x8Probs   = new byte[] { 100, 66 };
        private byte[] DefaultTx16x16Probs = new byte[] { 20, 152, 15, 101 };
        private byte[] DefaultTx32x32Probs = new byte[] { 3, 136, 37, 5, 52, 13 };

        private byte[] _defaultCoefProbs = new byte[]
        {
            195, 29,  183, 0, 84,  49,  136, 0, 8,   42,  71,  0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 31,  107, 169, 0, 35,  99,  159, 0,
            17,  82,  140, 0, 8,   66,  114, 0, 2,   44,  76,  0, 1,   19,  32,  0,
            40,  132, 201, 0, 29,  114, 187, 0, 13,  91,  157, 0, 7,   75,  127, 0,
            3,   58,  95,  0, 1,   28,  47,  0, 69,  142, 221, 0, 42,  122, 201, 0,
            15,  91,  159, 0, 6,   67,  121, 0, 1,   42,  77,  0, 1,   17,  31,  0,
            102, 148, 228, 0, 67,  117, 204, 0, 17,  82,  154, 0, 6,   59,  114, 0,
            2,   39,  75,  0, 1,   15,  29,  0, 156, 57,  233, 0, 119, 57,  212, 0,
            58,  48,  163, 0, 29,  40,  124, 0, 12,  30,  81,  0, 3,   12,  31,  0,
            191, 107, 226, 0, 124, 117, 204, 0, 25,  99,  155, 0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 29,  148, 210, 0, 37,  126, 194, 0,
            8,   93,  157, 0, 2,   68,  118, 0, 1,   39,  69,  0, 1,   17,  33,  0,
            41,  151, 213, 0, 27,  123, 193, 0, 3,   82,  144, 0, 1,   58,  105, 0,
            1,   32,  60,  0, 1,   13,  26,  0, 59,  159, 220, 0, 23,  126, 198, 0,
            4,   88,  151, 0, 1,   66,  114, 0, 1,   38,  71,  0, 1,   18,  34,  0,
            114, 136, 232, 0, 51,  114, 207, 0, 11,  83,  155, 0, 3,   56,  105, 0,
            1,   33,  65,  0, 1,   17,  34,  0, 149, 65,  234, 0, 121, 57,  215, 0,
            61,  49,  166, 0, 28,  36,  114, 0, 12,  25,  76,  0, 3,   16,  42,  0,
            214, 49,  220, 0, 132, 63,  188, 0, 42,  65,  137, 0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 85,  137, 221, 0, 104, 131, 216, 0,
            49,  111, 192, 0, 21,  87,  155, 0, 2,   49,  87,  0, 1,   16,  28,  0,
            89,  163, 230, 0, 90,  137, 220, 0, 29,  100, 183, 0, 10,  70,  135, 0,
            2,   42,  81,  0, 1,   17,  33,  0, 108, 167, 237, 0, 55,  133, 222, 0,
            15,  97,  179, 0, 4,   72,  135, 0, 1,   45,  85,  0, 1,   19,  38,  0,
            124, 146, 240, 0, 66,  124, 224, 0, 17,  88,  175, 0, 4,   58,  122, 0,
            1,   36,  75,  0, 1,   18,  37,  0, 141, 79,  241, 0, 126, 70,  227, 0,
            66,  58,  182, 0, 30,  44,  136, 0, 12,  34,  96,  0, 2,   20,  47,  0,
            229, 99,  249, 0, 143, 111, 235, 0, 46,  109, 192, 0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 82,  158, 236, 0, 94,  146, 224, 0,
            25,  117, 191, 0, 9,   87,  149, 0, 3,   56,  99,  0, 1,   33,  57,  0,
            83,  167, 237, 0, 68,  145, 222, 0, 10,  103, 177, 0, 2,   72,  131, 0,
            1,   41,  79,  0, 1,   20,  39,  0, 99,  167, 239, 0, 47,  141, 224, 0,
            10,  104, 178, 0, 2,   73,  133, 0, 1,   44,  85,  0, 1,   22,  47,  0,
            127, 145, 243, 0, 71,  129, 228, 0, 17,  93,  177, 0, 3,   61,  124, 0,
            1,   41,  84,  0, 1,   21,  52,  0, 157, 78,  244, 0, 140, 72,  231, 0,
            69,  58,  184, 0, 31,  44,  137, 0, 14,  38,  105, 0, 8,   23,  61,  0,
            125, 34,  187, 0, 52,  41,  133, 0, 6,   31,  56,  0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 37,  109, 153, 0, 51,  102, 147, 0,
            23,  87,  128, 0, 8,   67,  101, 0, 1,   41,  63,  0, 1,   19,  29,  0,
            31,  154, 185, 0, 17,  127, 175, 0, 6,   96,  145, 0, 2,   73,  114, 0,
            1,   51,  82,  0, 1,   28,  45,  0, 23,  163, 200, 0, 10,  131, 185, 0,
            2,   93,  148, 0, 1,   67,  111, 0, 1,   41,  69,  0, 1,   14,  24,  0,
            29,  176, 217, 0, 12,  145, 201, 0, 3,   101, 156, 0, 1,   69,  111, 0,
            1,   39,  63,  0, 1,   14,  23,  0, 57,  192, 233, 0, 25,  154, 215, 0,
            6,   109, 167, 0, 3,   78,  118, 0, 1,   48,  69,  0, 1,   21,  29,  0,
            202, 105, 245, 0, 108, 106, 216, 0, 18,  90,  144, 0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 33,  172, 219, 0, 64,  149, 206, 0,
            14,  117, 177, 0, 5,   90,  141, 0, 2,   61,  95,  0, 1,   37,  57,  0,
            33,  179, 220, 0, 11,  140, 198, 0, 1,   89,  148, 0, 1,   60,  104, 0,
            1,   33,  57,  0, 1,   12,  21,  0, 30,  181, 221, 0, 8,   141, 198, 0,
            1,   87,  145, 0, 1,   58,  100, 0, 1,   31,  55,  0, 1,   12,  20,  0,
            32,  186, 224, 0, 7,   142, 198, 0, 1,   86,  143, 0, 1,   58,  100, 0,
            1,   31,  55,  0, 1,   12,  22,  0, 57,  192, 227, 0, 20,  143, 204, 0,
            3,   96,  154, 0, 1,   68,  112, 0, 1,   42,  69,  0, 1,   19,  32,  0,
            212, 35,  215, 0, 113, 47,  169, 0, 29,  48,  105, 0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 74,  129, 203, 0, 106, 120, 203, 0,
            49,  107, 178, 0, 19,  84,  144, 0, 4,   50,  84,  0, 1,   15,  25,  0,
            71,  172, 217, 0, 44,  141, 209, 0, 15,  102, 173, 0, 6,   76,  133, 0,
            2,   51,  89,  0, 1,   24,  42,  0, 64,  185, 231, 0, 31,  148, 216, 0,
            8,   103, 175, 0, 3,   74,  131, 0, 1,   46,  81,  0, 1,   18,  30,  0,
            65,  196, 235, 0, 25,  157, 221, 0, 5,   105, 174, 0, 1,   67,  120, 0,
            1,   38,  69,  0, 1,   15,  30,  0, 65,  204, 238, 0, 30,  156, 224, 0,
            7,   107, 177, 0, 2,   70,  124, 0, 1,   42,  73,  0, 1,   18,  34,  0,
            225, 86,  251, 0, 144, 104, 235, 0, 42,  99,  181, 0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 85,  175, 239, 0, 112, 165, 229, 0,
            29,  136, 200, 0, 12,  103, 162, 0, 6,   77,  123, 0, 2,   53,  84,  0,
            75,  183, 239, 0, 30,  155, 221, 0, 3,   106, 171, 0, 1,   74,  128, 0,
            1,   44,  76,  0, 1,   17,  28,  0, 73,  185, 240, 0, 27,  159, 222, 0,
            2,   107, 172, 0, 1,   75,  127, 0, 1,   42,  73,  0, 1,   17,  29,  0,
            62,  190, 238, 0, 21,  159, 222, 0, 2,   107, 172, 0, 1,   72,  122, 0,
            1,   40,  71,  0, 1,   18,  32,  0, 61,  199, 240, 0, 27,  161, 226, 0,
            4,   113, 180, 0, 1,   76,  129, 0, 1,   46,  80,  0, 1,   23,  41,  0,
            7,   27,  153, 0, 5,   30,  95,  0, 1,   16,  30,  0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 50,  75,  127, 0, 57,  75,  124, 0,
            27,  67,  108, 0, 10,  54,  86,  0, 1,   33,  52,  0, 1,   12,  18,  0,
            43,  125, 151, 0, 26,  108, 148, 0, 7,   83,  122, 0, 2,   59,  89,  0,
            1,   38,  60,  0, 1,   17,  27,  0, 23,  144, 163, 0, 13,  112, 154, 0,
            2,   75,  117, 0, 1,   50,  81,  0, 1,   31,  51,  0, 1,   14,  23,  0,
            18,  162, 185, 0, 6,   123, 171, 0, 1,   78,  125, 0, 1,   51,  86,  0,
            1,   31,  54,  0, 1,   14,  23,  0, 15,  199, 227, 0, 3,   150, 204, 0,
            1,   91,  146, 0, 1,   55,  95,  0, 1,   30,  53,  0, 1,   11,  20,  0,
            19,  55,  240, 0, 19,  59,  196, 0, 3,   52,  105, 0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 41,  166, 207, 0, 104, 153, 199, 0,
            31,  123, 181, 0, 14,  101, 152, 0, 5,   72,  106, 0, 1,   36,  52,  0,
            35,  176, 211, 0, 12,  131, 190, 0, 2,   88,  144, 0, 1,   60,  101, 0,
            1,   36,  60,  0, 1,   16,  28,  0, 28,  183, 213, 0, 8,   134, 191, 0,
            1,   86,  142, 0, 1,   56,  96,  0, 1,   30,  53,  0, 1,   12,  20,  0,
            20,  190, 215, 0, 4,   135, 192, 0, 1,   84,  139, 0, 1,   53,  91,  0,
            1,   28,  49,  0, 1,   11,  20,  0, 13,  196, 216, 0, 2,   137, 192, 0,
            1,   86,  143, 0, 1,   57,  99,  0, 1,   32,  56,  0, 1,   13,  24,  0,
            211, 29,  217, 0, 96,  47,  156, 0, 22,  43,  87,  0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 78,  120, 193, 0, 111, 116, 186, 0,
            46,  102, 164, 0, 15,  80,  128, 0, 2,   49,  76,  0, 1,   18,  28,  0,
            71,  161, 203, 0, 42,  132, 192, 0, 10,  98,  150, 0, 3,   69,  109, 0,
            1,   44,  70,  0, 1,   18,  29,  0, 57,  186, 211, 0, 30,  140, 196, 0,
            4,   93,  146, 0, 1,   62,  102, 0, 1,   38,  65,  0, 1,   16,  27,  0,
            47,  199, 217, 0, 14,  145, 196, 0, 1,   88,  142, 0, 1,   57,  98,  0,
            1,   36,  62,  0, 1,   15,  26,  0, 26,  219, 229, 0, 5,   155, 207, 0,
            1,   94,  151, 0, 1,   60,  104, 0, 1,   36,  62,  0, 1,   16,  28,  0,
            233, 29,  248, 0, 146, 47,  220, 0, 43,  52,  140, 0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 100, 163, 232, 0, 179, 161, 222, 0,
            63,  142, 204, 0, 37,  113, 174, 0, 26,  89,  137, 0, 18,  68,  97,  0,
            85,  181, 230, 0, 32,  146, 209, 0, 7,   100, 164, 0, 3,   71,  121, 0,
            1,   45,  77,  0, 1,   18,  30,  0, 65,  187, 230, 0, 20,  148, 207, 0,
            2,   97,  159, 0, 1,   68,  116, 0, 1,   40,  70,  0, 1,   14,  29,  0,
            40,  194, 227, 0, 8,   147, 204, 0, 1,   94,  155, 0, 1,   65,  112, 0,
            1,   39,  66,  0, 1,   14,  26,  0, 16,  208, 228, 0, 3,   151, 207, 0,
            1,   98,  160, 0, 1,   67,  117, 0, 1,   41,  74,  0, 1,   17,  31,  0,
            17,  38,  140, 0, 7,   34,  80,  0, 1,   17,  29,  0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 37,  75,  128, 0, 41,  76,  128, 0,
            26,  66,  116, 0, 12,  52,  94,  0, 2,   32,  55,  0, 1,   10,  16,  0,
            50,  127, 154, 0, 37,  109, 152, 0, 16,  82,  121, 0, 5,   59,  85,  0,
            1,   35,  54,  0, 1,   13,  20,  0, 40,  142, 167, 0, 17,  110, 157, 0,
            2,   71,  112, 0, 1,   44,  72,  0, 1,   27,  45,  0, 1,   11,  17,  0,
            30,  175, 188, 0, 9,   124, 169, 0, 1,   74,  116, 0, 1,   48,  78,  0,
            1,   30,  49,  0, 1,   11,  18,  0, 10,  222, 223, 0, 2,   150, 194, 0,
            1,   83,  128, 0, 1,   48,  79,  0, 1,   27,  45,  0, 1,   11,  17,  0,
            36,  41,  235, 0, 29,  36,  193, 0, 10,  27,  111, 0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 85,  165, 222, 0, 177, 162, 215, 0,
            110, 135, 195, 0, 57,  113, 168, 0, 23,  83,  120, 0, 10,  49,  61,  0,
            85,  190, 223, 0, 36,  139, 200, 0, 5,   90,  146, 0, 1,   60,  103, 0,
            1,   38,  65,  0, 1,   18,  30,  0, 72,  202, 223, 0, 23,  141, 199, 0,
            2,   86,  140, 0, 1,   56,  97,  0, 1,   36,  61,  0, 1,   16,  27,  0,
            55,  218, 225, 0, 13,  145, 200, 0, 1,   86,  141, 0, 1,   57,  99,  0,
            1,   35,  61,  0, 1,   13,  22,  0, 15,  235, 212, 0, 1,   132, 184, 0,
            1,   84,  139, 0, 1,   57,  97,  0, 1,   34,  56,  0, 1,   14,  23,  0,
            181, 21,  201, 0, 61,  37,  123, 0, 10,  38,  71,  0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 47,  106, 172, 0, 95,  104, 173, 0,
            42,  93,  159, 0, 18,  77,  131, 0, 4,   50,  81,  0, 1,   17,  23,  0,
            62,  147, 199, 0, 44,  130, 189, 0, 28,  102, 154, 0, 18,  75,  115, 0,
            2,   44,  65,  0, 1,   12,  19,  0, 55,  153, 210, 0, 24,  130, 194, 0,
            3,   93,  146, 0, 1,   61,  97,  0, 1,   31,  50,  0, 1,   10,  16,  0,
            49,  186, 223, 0, 17,  148, 204, 0, 1,   96,  142, 0, 1,   53,  83,  0,
            1,   26,  44,  0, 1,   11,  17,  0, 13,  217, 212, 0, 2,   136, 180, 0,
            1,   78,  124, 0, 1,   50,  83,  0, 1,   29,  49,  0, 1,   14,  23,  0,
            197, 13,  247, 0, 82,  17,  222, 0, 25,  17,  162, 0, 0,   0,   0,   0,
            0,   0,   0,   0, 0,   0,   0,   0, 126, 186, 247, 0, 234, 191, 243, 0,
            176, 177, 234, 0, 104, 158, 220, 0, 66,  128, 186, 0, 55,  90,  137, 0,
            111, 197, 242, 0, 46,  158, 219, 0, 9,   104, 171, 0, 2,   65,  125, 0,
            1,   44,  80,  0, 1,   17,  91,  0, 104, 208, 245, 0, 39,  168, 224, 0,
            3,   109, 162, 0, 1,   79,  124, 0, 1,   50,  102, 0, 1,   43,  102, 0,
            84,  220, 246, 0, 31,  177, 231, 0, 2,   115, 180, 0, 1,   79,  134, 0,
            1,   55,  77,  0, 1,   60,  79,  0, 43,  243, 240, 0, 8,   180, 217, 0,
            1,   115, 166, 0, 1,   84,  121, 0, 1,   51,  67,  0, 1,   16,  6,   0
        };

        private byte[] _defaultSkipProbs = new byte[] { 192, 128, 64 };

        private byte[] _defaultInterModeProbs = new byte[]
        {
            2, 173, 34, 0, 7,  145, 85, 0, 7,  166, 63, 0, 7, 94, 66, 0,
            8, 64,  46, 0, 17, 81,  31, 0, 25, 29,  30, 0
        };

        private byte[] _defaultInterpFilterProbs = new byte[]
        {
            235, 162, 36, 255, 34, 3, 149, 144
        };

        private byte[] _defaultIsInterProbs = new byte[] { 9, 102, 187, 225 };

        private byte[] _defaultCompModeProbs = new byte[] { 239, 183, 119, 96, 41 };

        private byte[] _defaultSingleRefProbs = new byte[]
        {
            33, 16, 77, 74, 142, 142, 172, 170, 238, 247
        };

        private byte[] _defaultCompRefProbs = new byte[] { 50, 126, 123, 221, 226 };

        private byte[] _defaultYModeProbs0 = new byte[]
        {
            65,  32, 18, 144, 162, 194, 41, 51, 132, 68,  18, 165, 217, 196, 45, 40,
            173, 80, 19, 176, 240, 193, 64, 35, 221, 135, 38, 194, 248, 121, 96, 85
        };

        private byte[] _defaultYModeProbs1 = new byte[] { 98, 78, 46, 29 };

        private byte[] _defaultPartitionProbs = new byte[]
        {
            199, 122, 141, 0, 147, 63,  159, 0, 148, 133, 118, 0, 121, 104, 114, 0,
            174, 73,  87,  0, 92,  41,  83,  0, 82,  99,  50,  0, 53,  39,  39,  0,
            177, 58,  59,  0, 68,  26,  63,  0, 52,  79,  25,  0, 17,  14,  12,  0,
            222, 34,  30,  0, 72,  16,  44,  0, 58,  32,  12,  0, 10,  7,   6,   0
        };

        private byte[] _defaultMvJointProbs = new byte[] { 32, 64, 96 };

        private byte[] _defaultMvSignProbs = new byte[] { 128, 128 };

        private byte[] _defaultMvClassProbs = new byte[]
        {
            224, 144, 192, 168, 192, 176, 192, 198, 198, 245, 216, 128, 176, 160, 176, 176,
            192, 198, 198, 208
        };

        private byte[] _defaultMvClass0BitProbs = new byte[] { 216, 208 };

        private byte[] _defaultMvBitsProbs = new byte[]
        {
            136, 140, 148, 160, 176, 192, 224, 234, 234, 240, 136, 140, 148, 160, 176, 192,
            224, 234, 234, 240
        };

        private byte[] _defaultMvClass0FrProbs = new byte[]
        {
            128, 128, 64, 96, 112, 64, 128, 128, 64, 96, 112, 64
        };

        private byte[] _defaultMvFrProbs = new byte[] { 64, 96, 64, 64, 96, 64 };

        private byte[] _defaultMvClass0HpProbs = new byte[] { 160, 160 };

        private byte[] _defaultMvHpProbs = new byte[] { 128, 128 };

        private sbyte[] _loopFilterRefDeltas;
        private sbyte[] _loopFilterModeDeltas;

        private LinkedList<int> _frameSlotByLastUse;

        private Dictionary<long, LinkedListNode<int>> _cachedRefFrames;

        public Vp9Decoder()
        {
            _loopFilterRefDeltas  = new sbyte[4];
            _loopFilterModeDeltas = new sbyte[2];

            _frameSlotByLastUse = new LinkedList<int>();

            for (int slot = 0; slot < 8; slot++)
            {
                _frameSlotByLastUse.AddFirst(slot);
            }

            _cachedRefFrames = new Dictionary<long, LinkedListNode<int>>();
        }

        public void Decode(
            Vp9FrameKeys         keys,
            Vp9FrameHeader       header,
            Vp9ProbabilityTables probs,
            byte[]               frameData)
        {
            bool isKeyFrame         = ((header.Flags >> 0) & 1) != 0;
            bool lastIsKeyFrame     = ((header.Flags >> 1) & 1) != 0;
            bool frameSizeChanged   = ((header.Flags >> 2) & 1) != 0;
            bool errorResilientMode = ((header.Flags >> 3) & 1) != 0;
            bool lastShowFrame      = ((header.Flags >> 4) & 1) != 0;
            bool isFrameIntra       = ((header.Flags >> 5) & 1) != 0;

            bool showFrame = !isFrameIntra;

            //Write compressed header.
            byte[] compressedHeaderData;

            using (MemoryStream compressedHeader = new MemoryStream())
            {
                VpxRangeEncoder writer = new VpxRangeEncoder(compressedHeader);

                if (!header.Lossless)
                {
                    if ((uint)header.TxMode >= 3)
                    {
                        writer.Write(3, 2);
                        writer.Write(header.TxMode == 4);
                    }
                    else
                    {
                        writer.Write(header.TxMode, 2);
                    }
                }

                if (header.TxMode == 4)
                {
                    WriteProbabilityUpdate(writer, probs.Tx8x8Probs,   DefaultTx8x8Probs);
                    WriteProbabilityUpdate(writer, probs.Tx16x16Probs, DefaultTx16x16Probs);
                    WriteProbabilityUpdate(writer, probs.Tx32x32Probs, DefaultTx32x32Probs);
                }

                WriteCoefProbabilityUpdate(writer, header.TxMode, probs.CoefProbs, _defaultCoefProbs);

                WriteProbabilityUpdate(writer, probs.SkipProbs, _defaultSkipProbs);

                if (!isFrameIntra)
                {
                    WriteProbabilityUpdateAligned4(writer, probs.InterModeProbs, _defaultInterModeProbs);

                    if (header.RawInterpolationFilter == 4)
                    {
                        WriteProbabilityUpdate(writer, probs.InterpFilterProbs, _defaultInterpFilterProbs);
                    }

                    WriteProbabilityUpdate(writer, probs.IsInterProbs, _defaultIsInterProbs);

                    if ((header.RefFrameSignBias[1] & 1) != (header.RefFrameSignBias[2] & 1) ||
                        (header.RefFrameSignBias[1] & 1) != (header.RefFrameSignBias[3] & 1))
                    {
                        if ((uint)header.CompPredMode >= 1)
                        {
                            writer.Write(1, 1);
                            writer.Write(header.CompPredMode == 2);
                        }
                        else
                        {
                            writer.Write(0, 1);
                        }
                    }

                    if (header.CompPredMode == 2)
                    {
                        WriteProbabilityUpdate(writer, probs.CompModeProbs, _defaultCompModeProbs);
                    }

                    if (header.CompPredMode != 1)
                    {
                        WriteProbabilityUpdate(writer, probs.SingleRefProbs, _defaultSingleRefProbs);
                    }

                    if (header.CompPredMode != 0)
                    {
                        WriteProbabilityUpdate(writer, probs.CompRefProbs, _defaultCompRefProbs);
                    }

                    for (int index = 0; index < 4; index++)
                    {
                        int i = index * 8;
                        int j = index;

                        WriteProbabilityUpdate(writer, probs.YModeProbs0[i + 0], _defaultYModeProbs0[i + 0]);
                        WriteProbabilityUpdate(writer, probs.YModeProbs0[i + 1], _defaultYModeProbs0[i + 1]);
                        WriteProbabilityUpdate(writer, probs.YModeProbs0[i + 2], _defaultYModeProbs0[i + 2]);
                        WriteProbabilityUpdate(writer, probs.YModeProbs0[i + 3], _defaultYModeProbs0[i + 3]);
                        WriteProbabilityUpdate(writer, probs.YModeProbs0[i + 4], _defaultYModeProbs0[i + 4]);
                        WriteProbabilityUpdate(writer, probs.YModeProbs0[i + 5], _defaultYModeProbs0[i + 5]);
                        WriteProbabilityUpdate(writer, probs.YModeProbs0[i + 6], _defaultYModeProbs0[i + 6]);
                        WriteProbabilityUpdate(writer, probs.YModeProbs0[i + 7], _defaultYModeProbs0[i + 7]);
                        WriteProbabilityUpdate(writer, probs.YModeProbs1[j + 0], _defaultYModeProbs1[j + 0]);
                    }

                    WriteProbabilityUpdateAligned4(writer, probs.PartitionProbs, _defaultPartitionProbs);

                    for (int i = 0; i < 3; i++)
                    {
                        WriteMvProbabilityUpdate(writer, probs.MvJointProbs[i], _defaultMvJointProbs[i]);
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        WriteMvProbabilityUpdate(writer, probs.MvSignProbs[i], _defaultMvSignProbs[i]);

                        for (int j = 0; j < 10; j++)
                        {
                            int index = i * 10 + j;

                            WriteMvProbabilityUpdate(writer, probs.MvClassProbs[index], _defaultMvClassProbs[index]);
                        }

                        WriteMvProbabilityUpdate(writer, probs.MvClass0BitProbs[i], _defaultMvClass0BitProbs[i]);

                        for (int j = 0; j < 10; j++)
                        {
                            int index = i * 10 + j;

                            WriteMvProbabilityUpdate(writer, probs.MvBitsProbs[index], _defaultMvBitsProbs[index]);
                        }
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                int index = i * 2 * 3 + j * 3 + k;

                                WriteMvProbabilityUpdate(writer, probs.MvClass0FrProbs[index], _defaultMvClass0FrProbs[index]);
                            }
                        }

                        for (int j = 0; j < 3; j++)
                        {
                            int index = i * 3 + j;

                            WriteMvProbabilityUpdate(writer, probs.MvFrProbs[index], _defaultMvFrProbs[index]);
                        }
                    }

                    if (header.AllowHighPrecisionMv)
                    {
                        for (int index = 0; index < 2; index++)
                        {
                            WriteMvProbabilityUpdate(writer, probs.MvClass0HpProbs[index], _defaultMvClass0HpProbs[index]);
                            WriteMvProbabilityUpdate(writer, probs.MvHpProbs[index],       _defaultMvHpProbs[index]);
                        }
                    }
                }

                writer.End();

                compressedHeaderData = compressedHeader.ToArray();
            }

            //Write uncompressed header.
            using (MemoryStream encodedHeader = new MemoryStream())
            {
                VpxBitStreamWriter writer = new VpxBitStreamWriter(encodedHeader);

                writer.WriteU(2, 2); //Frame marker.
                writer.WriteU(0, 2); //Profile.
                writer.WriteBit(false); //Show existing frame.
                writer.WriteBit(!isKeyFrame);
                writer.WriteBit(showFrame);
                writer.WriteBit(errorResilientMode);

                if (isKeyFrame)
                {
                    writer.WriteU(FrameSyncCode, 24);
                    writer.WriteU(0, 3); //Color space.
                    writer.WriteU(0, 1); //Color range.
                    writer.WriteU(header.CurrentFrame.Width - 1, 16);
                    writer.WriteU(header.CurrentFrame.Height - 1, 16);
                    writer.WriteBit(false); //Render and frame size different.

                    _cachedRefFrames.Clear();

                    //On key frames, all frame slots are set to the current frame,
                    //so the value of the selected slot doesn't really matter.
                    GetNewFrameSlot(keys.CurrKey);
                }
                else
                {
                    if (!showFrame)
                    {
                        writer.WriteBit(isFrameIntra);
                    }

                    if (!errorResilientMode)
                    {
                        writer.WriteU(0, 2); //Reset frame context.
                    }

                    int refreshFrameFlags = 1 << GetNewFrameSlot(keys.CurrKey);

                    if (isFrameIntra)
                    {
                        writer.WriteU(FrameSyncCode, 24);
                        writer.WriteU(refreshFrameFlags, 8);
                        writer.WriteU(header.CurrentFrame.Width - 1, 16);
                        writer.WriteU(header.CurrentFrame.Height - 1, 16);
                        writer.WriteBit(false); //Render and frame size different.
                    }
                    else
                    {
                        writer.WriteU(refreshFrameFlags, 8);

                        int[] refFrameIndex = new int[]
                        {
                            GetFrameSlot(keys.Ref0Key),
                            GetFrameSlot(keys.Ref1Key),
                            GetFrameSlot(keys.Ref2Key)
                        };

                        byte[] refFrameSignBias = header.RefFrameSignBias;

                        for (int index = 1; index < 4; index++)
                        {
                            writer.WriteU(refFrameIndex[index - 1], 3);
                            writer.WriteU(refFrameSignBias[index], 1);
                        }

                        writer.WriteBit(true); //Frame size with refs.
                        writer.WriteBit(false); //Render and frame size different.
                        writer.WriteBit(header.AllowHighPrecisionMv);
                        writer.WriteBit(header.RawInterpolationFilter == 4);

                        if (header.RawInterpolationFilter != 4)
                        {
                            writer.WriteU(header.RawInterpolationFilter, 2);
                        }
                    }
                }

                if (!errorResilientMode)
                {
                    writer.WriteBit(false); //Refresh frame context.
                    writer.WriteBit(true); //Frame parallel decoding mode.
                }

                writer.WriteU(0, 2); //Frame context index.

                writer.WriteU(header.LoopFilterLevel, 6);
                writer.WriteU(header.LoopFilterSharpness, 3);
                writer.WriteBit(header.LoopFilterDeltaEnabled);

                if (header.LoopFilterDeltaEnabled)
                {
                    bool[] updateLoopFilterRefDeltas  = new bool[4];
                    bool[] updateLoopFilterModeDeltas = new bool[2];

                    bool loopFilterDeltaUpdate = false;

                    for (int index = 0; index < header.LoopFilterRefDeltas.Length; index++)
                    {
                        sbyte old =        _loopFilterRefDeltas[index];
                        sbyte New = header.LoopFilterRefDeltas[index];

                        loopFilterDeltaUpdate |= (updateLoopFilterRefDeltas[index] = old != New);
                    }

                    for (int index = 0; index < header.LoopFilterModeDeltas.Length; index++)
                    {
                        sbyte old =        _loopFilterModeDeltas[index];
                        sbyte New = header.LoopFilterModeDeltas[index];

                        loopFilterDeltaUpdate |= (updateLoopFilterModeDeltas[index] = old != New);
                    }

                    writer.WriteBit(loopFilterDeltaUpdate);

                    if (loopFilterDeltaUpdate)
                    {
                        for (int index = 0; index < header.LoopFilterRefDeltas.Length; index++)
                        {
                            writer.WriteBit(updateLoopFilterRefDeltas[index]);

                            if (updateLoopFilterRefDeltas[index])
                            {
                                writer.WriteS(header.LoopFilterRefDeltas[index], 6);
                            }
                        }

                        for (int index = 0; index < header.LoopFilterModeDeltas.Length; index++)
                        {
                            writer.WriteBit(updateLoopFilterModeDeltas[index]);

                            if (updateLoopFilterModeDeltas[index])
                            {
                                writer.WriteS(header.LoopFilterModeDeltas[index], 6);
                            }
                        }
                    }
                }

                writer.WriteU(header.BaseQIndex, 8);

                writer.WriteDeltaQ(header.DeltaQYDc);
                writer.WriteDeltaQ(header.DeltaQUvDc);
                writer.WriteDeltaQ(header.DeltaQUvAc);

                writer.WriteBit(false); //Segmentation enabled (TODO).

                int minTileColsLog2 = CalcMinLog2TileCols(header.CurrentFrame.Width);
                int maxTileColsLog2 = CalcMaxLog2TileCols(header.CurrentFrame.Width);

                int tileColsLog2Diff = header.TileColsLog2 - minTileColsLog2;

                int tileColsLog2IncMask = (1 << tileColsLog2Diff) - 1;

                //If it's less than the maximum, we need to add an extra 0 on the bitstream
                //to indicate that it should stop reading.
                if (header.TileColsLog2 < maxTileColsLog2)
                {
                    writer.WriteU(tileColsLog2IncMask << 1, tileColsLog2Diff + 1);
                }
                else
                {
                    writer.WriteU(tileColsLog2IncMask, tileColsLog2Diff);
                }

                bool tileRowsLog2IsNonZero = header.TileRowsLog2 != 0;

                writer.WriteBit(tileRowsLog2IsNonZero);

                if (tileRowsLog2IsNonZero)
                {
                    writer.WriteBit(header.TileRowsLog2 > 1);
                }

                writer.WriteU(compressedHeaderData.Length, 16);

                writer.Flush();

                encodedHeader.Write(compressedHeaderData, 0, compressedHeaderData.Length);

                if (!FFmpegWrapper.IsInitialized)
                {
                    FFmpegWrapper.Vp9Initialize();
                }

                FFmpegWrapper.DecodeFrame(DecoderHelper.Combine(encodedHeader.ToArray(), frameData));
            }

            _loopFilterRefDeltas  = header.LoopFilterRefDeltas;
            _loopFilterModeDeltas = header.LoopFilterModeDeltas;
        }

        private int GetNewFrameSlot(long key)
        {
            LinkedListNode<int> node = _frameSlotByLastUse.Last;

            _frameSlotByLastUse.RemoveLast();
            _frameSlotByLastUse.AddFirst(node);

            _cachedRefFrames[key] = node;

            return node.Value;
        }

        private int GetFrameSlot(long key)
        {
            if (_cachedRefFrames.TryGetValue(key, out LinkedListNode<int> node))
            {
                _frameSlotByLastUse.Remove(node);
                _frameSlotByLastUse.AddFirst(node);

                return node.Value;
            }

            //Reference frame was lost.
            //What we should do in this case?
            return 0;
        }

        private void WriteProbabilityUpdate(VpxRangeEncoder writer, byte[] New, byte[] old)
        {
            for (int offset = 0; offset < New.Length; offset++)
            {
                WriteProbabilityUpdate(writer, New[offset], old[offset]);
            }
        }

        private void WriteCoefProbabilityUpdate(VpxRangeEncoder writer, int txMode, byte[] New, byte[] old)
        {
            //Note: There's 1 byte added on each packet for alignment,
            //this byte is ignored when doing updates.
            const int blockBytes = 2 * 2 * 6 * 6 * 4;

            bool NeedsUpdate(int baseIndex)
            {
                int index = baseIndex;

                for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                for (int k = 0; k < 6; k++)
                for (int l = 0; l < 6; l++)
                {
                    if (New[index + 0] != old[index + 0] ||
                        New[index + 1] != old[index + 1] ||
                        New[index + 2] != old[index + 2])
                    {
                        return true;
                    }

                    index += 4;
                }

                return false;
            }

            for (int blockIndex = 0; blockIndex < 4; blockIndex++)
            {
                int baseIndex = blockIndex * blockBytes;

                bool update = NeedsUpdate(baseIndex);

                writer.Write(update);

                if (update)
                {
                    int index = baseIndex;

                    for (int i = 0; i < 2; i++)
                    for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 6; k++)
                    for (int l = 0; l < 6; l++)
                    {
                        if (k != 0 || l < 3)
                        {
                            WriteProbabilityUpdate(writer, New[index + 0], old[index + 0]);
                            WriteProbabilityUpdate(writer, New[index + 1], old[index + 1]);
                            WriteProbabilityUpdate(writer, New[index + 2], old[index + 2]);
                        }

                        index += 4;
                    }
                }

                if (blockIndex == txMode)
                {
                    break;
                }
            }
        }

        private void WriteProbabilityUpdateAligned4(VpxRangeEncoder writer, byte[] New, byte[] old)
        {
            for (int offset = 0; offset < New.Length; offset += 4)
            {
                WriteProbabilityUpdate(writer, New[offset + 0], old[offset + 0]);
                WriteProbabilityUpdate(writer, New[offset + 1], old[offset + 1]);
                WriteProbabilityUpdate(writer, New[offset + 2], old[offset + 2]);
            }
        }

        private void WriteProbabilityUpdate(VpxRangeEncoder writer, byte New, byte old)
        {
            bool update = New != old;

            writer.Write(update, DiffUpdateProbability);

            if (update)
            {
                WriteProbabilityDelta(writer, New, old);
            }
        }

        private void WriteProbabilityDelta(VpxRangeEncoder writer, int New, int old)
        {
            int delta = RemapProbability(New, old);

            EncodeTermSubExp(writer, delta);
        }

        private int RemapProbability(int New, int old)
        {
            New--;
            old--;

            int index;

            if (old * 2 <= 0xff)
            {
                index = RecenterNonNeg(New, old) - 1;
            }
            else
            {
                index = RecenterNonNeg(0xff - 1 - New, 0xff - 1 - old) - 1;
            }

            return MapLut[index];
        }

        private int RecenterNonNeg(int New, int old)
        {
            if (New > old * 2)
            {
                return New;
            }
            else if (New >= old)
            {
                return (New - old) * 2;
            }
            else /* if (New < Old) */
            {
                return (old - New) * 2 - 1;
            }
        }

        private void EncodeTermSubExp(VpxRangeEncoder writer, int value)
        {
            if (WriteLessThan(writer, value, 16))
            {
                writer.Write(value, 4);
            }
            else if (WriteLessThan(writer, value, 32))
            {
                writer.Write(value - 16, 4);
            }
            else if (WriteLessThan(writer, value, 64))
            {
                writer.Write(value - 32, 5);
            }
            else
            {
                value -= 64;

                const int size = 8;

                int mask = (1 << size) - 191;

                int delta = value - mask;

                if (delta < 0)
                {
                    writer.Write(value, size - 1);
                }
                else
                {
                    writer.Write(delta / 2 + mask, size - 1);
                    writer.Write(delta & 1, 1);
                }
            }
        }

        private bool WriteLessThan(VpxRangeEncoder writer, int value, int test)
        {
            bool isLessThan = value < test;

            writer.Write(!isLessThan);

            return isLessThan;
        }

        private void WriteMvProbabilityUpdate(VpxRangeEncoder writer, byte New, byte old)
        {
            bool update = New != old;

            writer.Write(update, DiffUpdateProbability);

            if (update)
            {
                writer.Write(New >> 1, 7);
            }
        }

        private static int CalcMinLog2TileCols(int frameWidth)
        {
            int sb64Cols = (frameWidth + 63) / 64;
            int minLog2  = 0;

            while ((64 << minLog2) < sb64Cols)
            {
                minLog2++;
            }

            return minLog2;
        }

        private static int CalcMaxLog2TileCols(int frameWidth)
        {
            int sb64Cols = (frameWidth + 63) / 64;
            int maxLog2  = 1;

            while ((sb64Cols >> maxLog2) >= 4)
            {
                maxLog2++;
            }

            return maxLog2 - 1;
        }
    }
}
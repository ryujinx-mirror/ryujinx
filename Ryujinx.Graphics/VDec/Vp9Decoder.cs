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

        private byte[] DefaultCoefProbs = new byte[]
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

        private byte[] DefaultSkipProbs = new byte[] { 192, 128, 64 };

        private byte[] DefaultInterModeProbs = new byte[]
        {
            2, 173, 34, 0, 7,  145, 85, 0, 7,  166, 63, 0, 7, 94, 66, 0,
            8, 64,  46, 0, 17, 81,  31, 0, 25, 29,  30, 0
        };

        private byte[] DefaultInterpFilterProbs = new byte[]
        {
            235, 162, 36, 255, 34, 3, 149, 144
        };

        private byte[] DefaultIsInterProbs = new byte[] { 9, 102, 187, 225 };

        private byte[] DefaultCompModeProbs = new byte[] { 239, 183, 119, 96, 41 };

        private byte[] DefaultSingleRefProbs = new byte[]
        {
            33, 16, 77, 74, 142, 142, 172, 170, 238, 247
        };

        private byte[] DefaultCompRefProbs = new byte[] { 50, 126, 123, 221, 226 };

        private byte[] DefaultYModeProbs0 = new byte[]
        {
            65,  32, 18, 144, 162, 194, 41, 51, 132, 68,  18, 165, 217, 196, 45, 40,
            173, 80, 19, 176, 240, 193, 64, 35, 221, 135, 38, 194, 248, 121, 96, 85
        };

        private byte[] DefaultYModeProbs1 = new byte[] { 98, 78, 46, 29 };

        private byte[] DefaultPartitionProbs = new byte[]
        {
            199, 122, 141, 0, 147, 63,  159, 0, 148, 133, 118, 0, 121, 104, 114, 0,
            174, 73,  87,  0, 92,  41,  83,  0, 82,  99,  50,  0, 53,  39,  39,  0,
            177, 58,  59,  0, 68,  26,  63,  0, 52,  79,  25,  0, 17,  14,  12,  0,
            222, 34,  30,  0, 72,  16,  44,  0, 58,  32,  12,  0, 10,  7,   6,   0
        };

        private byte[] DefaultMvJointProbs = new byte[] { 32, 64, 96 };

        private byte[] DefaultMvSignProbs = new byte[] { 128, 128 };

        private byte[] DefaultMvClassProbs = new byte[]
        {
            224, 144, 192, 168, 192, 176, 192, 198, 198, 245, 216, 128, 176, 160, 176, 176,
            192, 198, 198, 208
        };

        private byte[] DefaultMvClass0BitProbs = new byte[] { 216, 208 };

        private byte[] DefaultMvBitsProbs = new byte[]
        {
            136, 140, 148, 160, 176, 192, 224, 234, 234, 240, 136, 140, 148, 160, 176, 192,
            224, 234, 234, 240
        };

        private byte[] DefaultMvClass0FrProbs = new byte[]
        {
            128, 128, 64, 96, 112, 64, 128, 128, 64, 96, 112, 64
        };

        private byte[] DefaultMvFrProbs = new byte[] { 64, 96, 64, 64, 96, 64 };

        private byte[] DefaultMvClass0HpProbs = new byte[] { 160, 160 };

        private byte[] DefaultMvHpProbs = new byte[] { 128, 128 };

        private sbyte[] LoopFilterRefDeltas;
        private sbyte[] LoopFilterModeDeltas;

        private LinkedList<int> FrameSlotByLastUse;

        private Dictionary<long, LinkedListNode<int>> CachedRefFrames;

        public Vp9Decoder()
        {
            LoopFilterRefDeltas  = new sbyte[4];
            LoopFilterModeDeltas = new sbyte[2];

            FrameSlotByLastUse = new LinkedList<int>();

            for (int Slot = 0; Slot < 8; Slot++)
            {
                FrameSlotByLastUse.AddFirst(Slot);
            }

            CachedRefFrames = new Dictionary<long, LinkedListNode<int>>();
        }

        public void Decode(
            Vp9FrameKeys         Keys,
            Vp9FrameHeader       Header,
            Vp9ProbabilityTables Probs,
            byte[]               FrameData)
        {
            bool IsKeyFrame         = ((Header.Flags >> 0) & 1) != 0;
            bool LastIsKeyFrame     = ((Header.Flags >> 1) & 1) != 0;
            bool FrameSizeChanged   = ((Header.Flags >> 2) & 1) != 0;
            bool ErrorResilientMode = ((Header.Flags >> 3) & 1) != 0;
            bool LastShowFrame      = ((Header.Flags >> 4) & 1) != 0;
            bool IsFrameIntra       = ((Header.Flags >> 5) & 1) != 0;

            bool ShowFrame = !IsFrameIntra;

            //Write compressed header.
            byte[] CompressedHeaderData;

            using (MemoryStream CompressedHeader = new MemoryStream())
            {
                VpxRangeEncoder Writer = new VpxRangeEncoder(CompressedHeader);

                if (!Header.Lossless)
                {
                    if ((uint)Header.TxMode >= 3)
                    {
                        Writer.Write(3, 2);
                        Writer.Write(Header.TxMode == 4);
                    }
                    else
                    {
                        Writer.Write(Header.TxMode, 2);
                    }
                }

                if (Header.TxMode == 4)
                {
                    WriteProbabilityUpdate(Writer, Probs.Tx8x8Probs,   DefaultTx8x8Probs);
                    WriteProbabilityUpdate(Writer, Probs.Tx16x16Probs, DefaultTx16x16Probs);
                    WriteProbabilityUpdate(Writer, Probs.Tx32x32Probs, DefaultTx32x32Probs);
                }

                WriteCoefProbabilityUpdate(Writer, Header.TxMode, Probs.CoefProbs, DefaultCoefProbs);

                WriteProbabilityUpdate(Writer, Probs.SkipProbs, DefaultSkipProbs);

                if (!IsFrameIntra)
                {
                    WriteProbabilityUpdateAligned4(Writer, Probs.InterModeProbs, DefaultInterModeProbs);

                    if (Header.RawInterpolationFilter == 4)
                    {
                        WriteProbabilityUpdate(Writer, Probs.InterpFilterProbs, DefaultInterpFilterProbs);
                    }

                    WriteProbabilityUpdate(Writer, Probs.IsInterProbs, DefaultIsInterProbs);

                    if ((Header.RefFrameSignBias[1] & 1) != (Header.RefFrameSignBias[2] & 1) ||
                        (Header.RefFrameSignBias[1] & 1) != (Header.RefFrameSignBias[3] & 1))
                    {
                        if ((uint)Header.CompPredMode >= 1)
                        {
                            Writer.Write(1, 1);
                            Writer.Write(Header.CompPredMode == 2);
                        }
                        else
                        {
                            Writer.Write(0, 1);
                        }
                    }

                    if (Header.CompPredMode == 2)
                    {
                        WriteProbabilityUpdate(Writer, Probs.CompModeProbs, DefaultCompModeProbs);
                    }

                    if (Header.CompPredMode != 1)
                    {
                        WriteProbabilityUpdate(Writer, Probs.SingleRefProbs, DefaultSingleRefProbs);
                    }

                    if (Header.CompPredMode != 0)
                    {
                        WriteProbabilityUpdate(Writer, Probs.CompRefProbs, DefaultCompRefProbs);
                    }

                    for (int Index = 0; Index < 4; Index++)
                    {
                        int i = Index * 8;
                        int j = Index;

                        WriteProbabilityUpdate(Writer, Probs.YModeProbs0[i + 0], DefaultYModeProbs0[i + 0]);
                        WriteProbabilityUpdate(Writer, Probs.YModeProbs0[i + 1], DefaultYModeProbs0[i + 1]);
                        WriteProbabilityUpdate(Writer, Probs.YModeProbs0[i + 2], DefaultYModeProbs0[i + 2]);
                        WriteProbabilityUpdate(Writer, Probs.YModeProbs0[i + 3], DefaultYModeProbs0[i + 3]);
                        WriteProbabilityUpdate(Writer, Probs.YModeProbs0[i + 4], DefaultYModeProbs0[i + 4]);
                        WriteProbabilityUpdate(Writer, Probs.YModeProbs0[i + 5], DefaultYModeProbs0[i + 5]);
                        WriteProbabilityUpdate(Writer, Probs.YModeProbs0[i + 6], DefaultYModeProbs0[i + 6]);
                        WriteProbabilityUpdate(Writer, Probs.YModeProbs0[i + 7], DefaultYModeProbs0[i + 7]);
                        WriteProbabilityUpdate(Writer, Probs.YModeProbs1[j + 0], DefaultYModeProbs1[j + 0]);
                    }

                    WriteProbabilityUpdateAligned4(Writer, Probs.PartitionProbs, DefaultPartitionProbs);

                    for (int i = 0; i < 3; i++)
                    {
                        WriteMvProbabilityUpdate(Writer, Probs.MvJointProbs[i], DefaultMvJointProbs[i]);
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        WriteMvProbabilityUpdate(Writer, Probs.MvSignProbs[i], DefaultMvSignProbs[i]);

                        for (int j = 0; j < 10; j++)
                        {
                            int Index = i * 10 + j;

                            WriteMvProbabilityUpdate(Writer, Probs.MvClassProbs[Index], DefaultMvClassProbs[Index]);
                        }

                        WriteMvProbabilityUpdate(Writer, Probs.MvClass0BitProbs[i], DefaultMvClass0BitProbs[i]);

                        for (int j = 0; j < 10; j++)
                        {
                            int Index = i * 10 + j;

                            WriteMvProbabilityUpdate(Writer, Probs.MvBitsProbs[Index], DefaultMvBitsProbs[Index]);
                        }
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                int Index = i * 2 * 3 + j * 3 + k;

                                WriteMvProbabilityUpdate(Writer, Probs.MvClass0FrProbs[Index], DefaultMvClass0FrProbs[Index]);
                            }
                        }

                        for (int j = 0; j < 3; j++)
                        {
                            int Index = i * 3 + j;

                            WriteMvProbabilityUpdate(Writer, Probs.MvFrProbs[Index], DefaultMvFrProbs[Index]);
                        }
                    }

                    if (Header.AllowHighPrecisionMv)
                    {
                        for (int Index = 0; Index < 2; Index++)
                        {
                            WriteMvProbabilityUpdate(Writer, Probs.MvClass0HpProbs[Index], DefaultMvClass0HpProbs[Index]);
                            WriteMvProbabilityUpdate(Writer, Probs.MvHpProbs[Index],       DefaultMvHpProbs[Index]);
                        }
                    }
                }

                Writer.End();

                CompressedHeaderData = CompressedHeader.ToArray();
            }

            //Write uncompressed header.
            using (MemoryStream EncodedHeader = new MemoryStream())
            {
                VpxBitStreamWriter Writer = new VpxBitStreamWriter(EncodedHeader);

                Writer.WriteU(2, 2); //Frame marker.
                Writer.WriteU(0, 2); //Profile.
                Writer.WriteBit(false); //Show existing frame.
                Writer.WriteBit(!IsKeyFrame);
                Writer.WriteBit(ShowFrame);
                Writer.WriteBit(ErrorResilientMode);

                if (IsKeyFrame)
                {
                    Writer.WriteU(FrameSyncCode, 24);
                    Writer.WriteU(0, 3); //Color space.
                    Writer.WriteU(0, 1); //Color range.
                    Writer.WriteU(Header.CurrentFrame.Width - 1, 16);
                    Writer.WriteU(Header.CurrentFrame.Height - 1, 16);
                    Writer.WriteBit(false); //Render and frame size different.

                    CachedRefFrames.Clear();

                    //On key frames, all frame slots are set to the current frame,
                    //so the value of the selected slot doesn't really matter.
                    GetNewFrameSlot(Keys.CurrKey);
                }
                else
                {
                    if (!ShowFrame)
                    {
                        Writer.WriteBit(IsFrameIntra);
                    }

                    if (!ErrorResilientMode)
                    {
                        Writer.WriteU(0, 2); //Reset frame context.
                    }

                    int RefreshFrameFlags = 1 << GetNewFrameSlot(Keys.CurrKey);

                    if (IsFrameIntra)
                    {
                        Writer.WriteU(FrameSyncCode, 24);
                        Writer.WriteU(RefreshFrameFlags, 8);
                        Writer.WriteU(Header.CurrentFrame.Width - 1, 16);
                        Writer.WriteU(Header.CurrentFrame.Height - 1, 16);
                        Writer.WriteBit(false); //Render and frame size different.
                    }
                    else
                    {
                        Writer.WriteU(RefreshFrameFlags, 8);

                        int[] RefFrameIndex = new int[]
                        {
                            GetFrameSlot(Keys.Ref0Key),
                            GetFrameSlot(Keys.Ref1Key),
                            GetFrameSlot(Keys.Ref2Key)
                        };

                        byte[] RefFrameSignBias = Header.RefFrameSignBias;

                        for (int Index = 1; Index < 4; Index++)
                        {
                            Writer.WriteU(RefFrameIndex[Index - 1], 3);
                            Writer.WriteU(RefFrameSignBias[Index], 1);
                        }

                        Writer.WriteBit(true); //Frame size with refs.
                        Writer.WriteBit(false); //Render and frame size different.
                        Writer.WriteBit(Header.AllowHighPrecisionMv);
                        Writer.WriteBit(Header.RawInterpolationFilter == 4);

                        if (Header.RawInterpolationFilter != 4)
                        {
                            Writer.WriteU(Header.RawInterpolationFilter, 2);
                        }
                    }
                }

                if (!ErrorResilientMode)
                {
                    Writer.WriteBit(false); //Refresh frame context.
                    Writer.WriteBit(true); //Frame parallel decoding mode.
                }

                Writer.WriteU(0, 2); //Frame context index.

                Writer.WriteU(Header.LoopFilterLevel, 6);
                Writer.WriteU(Header.LoopFilterSharpness, 3);
                Writer.WriteBit(Header.LoopFilterDeltaEnabled);

                if (Header.LoopFilterDeltaEnabled)
                {
                    bool[] UpdateLoopFilterRefDeltas  = new bool[4];
                    bool[] UpdateLoopFilterModeDeltas = new bool[2];

                    bool LoopFilterDeltaUpdate = false;

                    for (int Index = 0; Index < Header.LoopFilterRefDeltas.Length; Index++)
                    {
                        sbyte Old =        LoopFilterRefDeltas[Index];
                        sbyte New = Header.LoopFilterRefDeltas[Index];

                        LoopFilterDeltaUpdate |= (UpdateLoopFilterRefDeltas[Index] = Old != New);
                    }

                    for (int Index = 0; Index < Header.LoopFilterModeDeltas.Length; Index++)
                    {
                        sbyte Old =        LoopFilterModeDeltas[Index];
                        sbyte New = Header.LoopFilterModeDeltas[Index];

                        LoopFilterDeltaUpdate |= (UpdateLoopFilterModeDeltas[Index] = Old != New);
                    }

                    Writer.WriteBit(LoopFilterDeltaUpdate);

                    if (LoopFilterDeltaUpdate)
                    {
                        for (int Index = 0; Index < Header.LoopFilterRefDeltas.Length; Index++)
                        {
                            Writer.WriteBit(UpdateLoopFilterRefDeltas[Index]);

                            if (UpdateLoopFilterRefDeltas[Index])
                            {
                                Writer.WriteS(Header.LoopFilterRefDeltas[Index], 6);
                            }
                        }

                        for (int Index = 0; Index < Header.LoopFilterModeDeltas.Length; Index++)
                        {
                            Writer.WriteBit(UpdateLoopFilterModeDeltas[Index]);

                            if (UpdateLoopFilterModeDeltas[Index])
                            {
                                Writer.WriteS(Header.LoopFilterModeDeltas[Index], 6);
                            }
                        }
                    }
                }

                Writer.WriteU(Header.BaseQIndex, 8);

                Writer.WriteDeltaQ(Header.DeltaQYDc);
                Writer.WriteDeltaQ(Header.DeltaQUvDc);
                Writer.WriteDeltaQ(Header.DeltaQUvAc);

                Writer.WriteBit(false); //Segmentation enabled (TODO).

                int MinTileColsLog2 = CalcMinLog2TileCols(Header.CurrentFrame.Width);
                int MaxTileColsLog2 = CalcMaxLog2TileCols(Header.CurrentFrame.Width);

                int TileColsLog2Diff = Header.TileColsLog2 - MinTileColsLog2;

                int TileColsLog2IncMask = (1 << TileColsLog2Diff) - 1;

                //If it's less than the maximum, we need to add an extra 0 on the bitstream
                //to indicate that it should stop reading.
                if (Header.TileColsLog2 < MaxTileColsLog2)
                {
                    Writer.WriteU(TileColsLog2IncMask << 1, TileColsLog2Diff + 1);
                }
                else
                {
                    Writer.WriteU(TileColsLog2IncMask, TileColsLog2Diff);
                }

                bool TileRowsLog2IsNonZero = Header.TileRowsLog2 != 0;

                Writer.WriteBit(TileRowsLog2IsNonZero);

                if (TileRowsLog2IsNonZero)
                {
                    Writer.WriteBit(Header.TileRowsLog2 > 1);
                }

                Writer.WriteU(CompressedHeaderData.Length, 16);

                Writer.Flush();

                EncodedHeader.Write(CompressedHeaderData, 0, CompressedHeaderData.Length);

                if (!FFmpegWrapper.IsInitialized)
                {
                    FFmpegWrapper.Vp9Initialize();
                }

                FFmpegWrapper.DecodeFrame(DecoderHelper.Combine(EncodedHeader.ToArray(), FrameData));
            }

            LoopFilterRefDeltas  = Header.LoopFilterRefDeltas;
            LoopFilterModeDeltas = Header.LoopFilterModeDeltas;
        }

        private int GetNewFrameSlot(long Key)
        {
            LinkedListNode<int> Node = FrameSlotByLastUse.Last;

            FrameSlotByLastUse.RemoveLast();
            FrameSlotByLastUse.AddFirst(Node);

            CachedRefFrames[Key] = Node;

            return Node.Value;
        }

        private int GetFrameSlot(long Key)
        {
            if (CachedRefFrames.TryGetValue(Key, out LinkedListNode<int> Node))
            {
                FrameSlotByLastUse.Remove(Node);
                FrameSlotByLastUse.AddFirst(Node);

                return Node.Value;
            }

            //Reference frame was lost.
            //What we should do in this case?
            return 0;
        }

        private void WriteProbabilityUpdate(VpxRangeEncoder Writer, byte[] New, byte[] Old)
        {
            for (int Offset = 0; Offset < New.Length; Offset++)
            {
                WriteProbabilityUpdate(Writer, New[Offset], Old[Offset]);
            }
        }

        private void WriteCoefProbabilityUpdate(VpxRangeEncoder Writer, int TxMode, byte[] New, byte[] Old)
        {
            //Note: There's 1 byte added on each packet for alignment,
            //this byte is ignored when doing updates.
            const int BlockBytes = 2 * 2 * 6 * 6 * 4;

            bool NeedsUpdate(int BaseIndex)
            {
                int Index = BaseIndex;

                for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                for (int k = 0; k < 6; k++)
                for (int l = 0; l < 6; l++)
                {
                    if (New[Index + 0] != Old[Index + 0] ||
                        New[Index + 1] != Old[Index + 1] ||
                        New[Index + 2] != Old[Index + 2])
                    {
                        return true;
                    }

                    Index += 4;
                }

                return false;
            }

            for (int BlockIndex = 0; BlockIndex < 4; BlockIndex++)
            {
                int BaseIndex = BlockIndex * BlockBytes;

                bool Update = NeedsUpdate(BaseIndex);

                Writer.Write(Update);

                if (Update)
                {
                    int Index = BaseIndex;

                    for (int i = 0; i < 2; i++)
                    for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 6; k++)
                    for (int l = 0; l < 6; l++)
                    {
                        if (k != 0 || l < 3)
                        {
                            WriteProbabilityUpdate(Writer, New[Index + 0], Old[Index + 0]);
                            WriteProbabilityUpdate(Writer, New[Index + 1], Old[Index + 1]);
                            WriteProbabilityUpdate(Writer, New[Index + 2], Old[Index + 2]);
                        }

                        Index += 4;
                    }
                }

                if (BlockIndex == TxMode)
                {
                    break;
                }
            }
        }

        private void WriteProbabilityUpdateAligned4(VpxRangeEncoder Writer, byte[] New, byte[] Old)
        {
            for (int Offset = 0; Offset < New.Length; Offset += 4)
            {
                WriteProbabilityUpdate(Writer, New[Offset + 0], Old[Offset + 0]);
                WriteProbabilityUpdate(Writer, New[Offset + 1], Old[Offset + 1]);
                WriteProbabilityUpdate(Writer, New[Offset + 2], Old[Offset + 2]);
            }
        }

        private void WriteProbabilityUpdate(VpxRangeEncoder Writer, byte New, byte Old)
        {
            bool Update = New != Old;

            Writer.Write(Update, DiffUpdateProbability);

            if (Update)
            {
                WriteProbabilityDelta(Writer, New, Old);
            }
        }

        private void WriteProbabilityDelta(VpxRangeEncoder Writer, int New, int Old)
        {
            int Delta = RemapProbability(New, Old);

            EncodeTermSubExp(Writer, Delta);
        }

        private int RemapProbability(int New, int Old)
        {
            New--;
            Old--;

            int Index;

            if (Old * 2 <= 0xff)
            {
                Index = RecenterNonNeg(New, Old) - 1;
            }
            else
            {
                Index = RecenterNonNeg(0xff - 1 - New, 0xff - 1 - Old) - 1;
            }

            return MapLut[Index];
        }

        private int RecenterNonNeg(int New, int Old)
        {
            if (New > Old * 2)
            {
                return New;
            }
            else if (New >= Old)
            {
                return (New - Old) * 2;
            }
            else /* if (New < Old) */
            {
                return (Old - New) * 2 - 1;
            }
        }

        private void EncodeTermSubExp(VpxRangeEncoder Writer, int Value)
        {
            if (WriteLessThan(Writer, Value, 16))
            {
                Writer.Write(Value, 4);
            }
            else if (WriteLessThan(Writer, Value, 32))
            {
                Writer.Write(Value - 16, 4);
            }
            else if (WriteLessThan(Writer, Value, 64))
            {
                Writer.Write(Value - 32, 5);
            }
            else
            {
                Value -= 64;

                const int Size = 8;

                int Mask = (1 << Size) - 191;

                int Delta = Value - Mask;

                if (Delta < 0)
                {
                    Writer.Write(Value, Size - 1);
                }
                else
                {
                    Writer.Write(Delta / 2 + Mask, Size - 1);
                    Writer.Write(Delta & 1, 1);
                }
            }
        }

        private bool WriteLessThan(VpxRangeEncoder Writer, int Value, int Test)
        {
            bool IsLessThan = Value < Test;

            Writer.Write(!IsLessThan);

            return IsLessThan;
        }

        private void WriteMvProbabilityUpdate(VpxRangeEncoder Writer, byte New, byte Old)
        {
            bool Update = New != Old;

            Writer.Write(Update, DiffUpdateProbability);

            if (Update)
            {
                Writer.Write(New >> 1, 7);
            }
        }

        private static int CalcMinLog2TileCols(int FrameWidth)
        {
            int Sb64Cols = (FrameWidth + 63) / 64;
            int MinLog2  = 0;

            while ((64 << MinLog2) < Sb64Cols)
            {
                MinLog2++;
            }

            return MinLog2;
        }

        private static int CalcMaxLog2TileCols(int FrameWidth)
        {
            int Sb64Cols = (FrameWidth + 63) / 64;
            int MaxLog2  = 1;

            while ((Sb64Cols >> MaxLog2) >= 4)
            {
                MaxLog2++;
            }

            return MaxLog2 - 1;
        }
    }
}
int Helper_MultiplyHighS32(int x, int y)
{
    int msb;
    int lsb;
    imulExtended(x, y, msb, lsb);
    return msb;
}
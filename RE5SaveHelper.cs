namespace RE5SteamIdSwapper;

/// <summary>
/// Lógica de decodificação, codificação e checksum do save do RE5.
/// Baseada no formato Steam save (XOR chaining com magic key).
/// </summary>
public static class RE5SaveHelper
{
    // Chave mágica usada no XOR chaining
    private static readonly byte[] MagicKey = [0x00, 0x21, 0x11, 0x08, 0xC0, 0x4B, 0x00, 0x00];

    // Decodifica o arquivo: XOR chaining onde a key do próximo bloco é o bloco ENCODED anterior
    public static byte[] DecodeSaveData(byte[] data)
    {
        byte[] output = new byte[data.Length];
        byte[] key = (byte[])MagicKey.Clone();

        for (int i = 0; i < data.Length / 8; i++)
        {
            byte[] encoded = data.AsSpan(i * 8, 8).ToArray();
            byte[] decoded = XorBlocks(encoded, key);
            decoded.CopyTo(output, i * 8);
            key = encoded; // próxima key = bloco encoded atual
        }

        return output;
    }

    // Codifica o arquivo: XOR chaining onde a key do próximo bloco é o bloco ENCODED gerado
    public static byte[] EncodeSaveData(byte[] data)
    {
        byte[] output = new byte[data.Length];
        byte[] key = (byte[])MagicKey.Clone();

        for (int i = 0; i < data.Length / 8; i++)
        {
            byte[] decoded = data.AsSpan(i * 8, 8).ToArray();
            byte[] encoded = XorBlocks(decoded, key);
            encoded.CopyTo(output, i * 8);
            key = encoded; // próxima key = bloco encoded gerado
        }

        return output;
    }

    // Calcula o checksum do buffer decodificado (soma de UInt32LE em dois intervalos)
    public static uint CalculateChecksum(byte[] buffer)
    {
        uint checksum = ChecksumRange(buffer, 0x10, 0xEE7, 0);
        checksum = ChecksumRange(buffer, 0x3BB0, 0x5C0, checksum);
        return checksum;
    }

    // Aplica o novo Steam ID, recalcula o checksum e codifica o arquivo
    public static byte[] ProcessSave(byte[] rawData, ulong newSteamId)
    {
        byte[] decoded = DecodeSaveData(rawData);

        // Steam ID fica no offset 0x00 (64-bit little-endian)
        WriteUInt64LE(decoded, 0x00, newSteamId);

        // Checksum fica no offset 0x08 (32-bit little-endian)
        uint checksum = CalculateChecksum(decoded);
        WriteUInt32LE(decoded, 0x08, checksum);

        return EncodeSaveData(decoded);
    }

    // ── Helpers privados ────────────────────────────────────────────────────

    private static byte[] XorBlocks(byte[] a, byte[] b)
    {
        byte[] result = new byte[8];
        for (int i = 0; i < 8; i++)
            result[i] = (byte)(a[i] ^ b[i]);
        return result;
    }

    private static uint ChecksumRange(byte[] buffer, int offset, int iterations, uint current)
    {
        unchecked
        {
            for (int i = 0; i < iterations; i++)
                current += BitConverter.ToUInt32(buffer, offset + i * 4);
        }
        return current;
    }

    private static void WriteUInt64LE(byte[] buffer, int offset, ulong value)
    {
        for (int i = 0; i < 8; i++)
        {
            buffer[offset + i] = (byte)(value & 0xFF);
            value >>= 8;
        }
    }

    private static void WriteUInt32LE(byte[] buffer, int offset, uint value)
    {
        for (int i = 0; i < 4; i++)
        {
            buffer[offset + i] = (byte)(value & 0xFF);
            value >>= 8;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Strilanc.LinqToCollections;

public static class MainHash {
    public const string CharSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()_+-=|[];',.{}:<>? ";
    public const char CharNotInSet = 'Ʉ';
    //public static readonly Int32[] DataRange = Encode(CharSet + CharNotInSet).ToArray();
    public static readonly Int32[] DataRange = Encode(CharSet + CharNotInSet).ToArray();
    public static readonly Int32 MaxDataValue = DataRange.Max();
    public static readonly Int32 MinDataValue = DataRange.Min();

    public static void Main() {
        unchecked {
            var passwordHash = new HashState((int)0xDF8BEDAAu, (int)0xB5A86DDEu);
            var nameHash1 = new HashState((int)0xAD414D7Du, (int)0x8CC36A67u);
            var nameHash2 = new HashState(0x605D4A4F, 0x7EDDB1E5); // Procyon
            var nameHash3 = new HashState(0x3D10F092, 0x60084719);

            Console.WriteLine("n1: Hash({0}): {1} == {2}", "hRlGz%W3&R", Hash(Encode("hRlGz%W3&R")), nameHash1);
            Console.WriteLine("n2: Hash({0}): {1} == {2}", "Procyon", Hash(Encode("Procyon")), nameHash2);
            Console.WriteLine("n3: Hash({0}): {1} == {2}", "b>4FXV'Xf8", Hash(Encode("b>4FXV'Xf8")), nameHash3);
            Console.WriteLine("pass: Hash({0}): {1} == {2}", "<+nt1AkgbMht", Hash(Encode("<+nt1AkgbMht")), passwordHash);

            var knownPrefix = "";
            var start = Hash(Encode(knownPrefix));
            var dest = nameHash1;
            var solution = 
                11.Range()
                .Select(assumedLength => Hash4.Break(dest, assumedLength, false, start))
                .FirstOrDefault(e => e != null);
            Console.WriteLine("solution: " + Decode(solution));

            Console.ReadLine();
        }
    }

    public static Int32 Encode(char c) {
        return CharSet.Contains(c) ? CharSet.IndexOf(c) : CharSet.Length + 1;
    }
    public static IEnumerable<Int32> Encode(string text) {
        return text.Select(Encode);
    }
    public static char Decode(Int32 value) {
        return value == CharSet.Length + 1 ? CharNotInSet : CharSet[value];
    }
    public static string Decode(IEnumerable<Int32> value) {
        if (value == null) return null;
        return String.Join("", value.Select(Decode));
    }

    public static bool HashMatches(string text, params HashState[] validHashes) {
        return validHashes.Contains(Hash(Encode(text)));
    }
    public static bool Verify(string chat, string playerName) {
        unchecked {
            return chat.StartsWith("<+")
                && HashMatches(chat, new HashState((int)0xDF8BEDAAu, (int)0xB5A86DDEu))
                && HashMatches(playerName, new HashState((int)0xAD414D7Du, (int)0x8CC36A67u),
                                           new HashState(0x605D4A4F, 0x7EDDB1E5),
                                           new HashState(0x3D10F092, 0x60084719));
        }
    }
    public static HashState Hash(IEnumerable<Int32> data) {
        unchecked {
            return data.Aggregate(new HashState(), (h, e) => h.Advance(e));
        }
    }
}

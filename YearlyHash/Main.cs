﻿using System;
using System.Collections.Generic;
using System.Linq;

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

            //var rName2 = 9.Range().Select(e => Hash4.Break(nameHash2, e)).FirstOrDefault(e => e != null);
            //Console.WriteLine("n2: " + Decode(rName2));
            Console.WriteLine("n2: Hash({0}): {1} == {2}", "Procyon", Hash(Encode("Procyon")), nameHash2);

            //var rName1 = 9.Range().Select(e => Hash4.Break(nameHash1, e)).FirstOrDefault(e => e != null);
            //Console.WriteLine("n1: " + Decode(rName1));


            //var rPass = 9.Range().Select(e => Hash4.Break(passwordHash, e, Hash(Encode("<+")))).FirstOrDefault(e => e != null);
            var rPass = Hash4.Break(passwordHash, 9, cache: true, start: Hash(Encode("<+")));
            Console.WriteLine("pass: " + Decode(rPass));

            //var rName3 = 9.Range().Select(e => Hash4.Break(nameHash3, e)).FirstOrDefault(e => e != null);
            //Console.WriteLine("n3: " + Decode(rName3));

            
            //var testHash = Hash(Encode("iampied"));
            //var test = 7.Range().Select(e => Hash4.Break(testHash, e)).FirstOrDefault(e => e != null);
            //Console.WriteLine("test: " + Decode(test));
            //Console.WriteLine("done");

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

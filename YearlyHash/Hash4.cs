using System;
using System.Collections.Generic;

class LinearTracker {
    private readonly IReadOnlyDictionary<string, int> values;
}
static class Hash4 {
    public static HashState Hash(IEnumerable<Int32> data) {
        var a = 0;
        var b = 0;
        unchecked {
            foreach (var e in data) {
                for (var i = 0; i < 17; i++) {
                    a *= -6;
                    a += b;
                    a += 0x74FA;
                    a -= e;
                    b /= 3;
                    b += a;
                    b += 0x81BE;
                    b -= e;
                }
            }
        }
        return new HashState(a, b);
    }
}

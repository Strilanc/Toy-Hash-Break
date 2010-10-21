using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Hash3 {
    public static HashState Hash(IEnumerable<Int32> data) {
        unchecked {
            Int32 a = 0;
            Int32 b = 0;
            foreach (var e in data) {
                for (var i = 0; i < 17; i++) {
                    a = a * -6 + b + 0x74FA - e;
                    b = b / 3 + a + 0x81BE - e;
                }
            }
            return new HashState(a, b);
        }
    }
}

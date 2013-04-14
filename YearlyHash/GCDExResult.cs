using System;

public struct GCDExResult : IEquatable<GCDExResult> {
    public readonly Int64 x;
    public readonly Int64 y;
    public readonly Int64 g;
    public GCDExResult(Int64 g, Int64 x, Int64 y) {
        this.x = x;
        this.y = y;
        this.g = g;
    }
    public bool Equals(GCDExResult other) {
        return this.x == other.x && this.y == other.y && this.g == other.g;
    }
    public override bool Equals(Object obj) {
        return obj is GCDExResult && this.Equals((GCDExResult)obj);
    }
    public override int GetHashCode() {
        return this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.g.GetHashCode();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileBaseValue : IValue<TileBase>
{
    private TileBase tileBase;

    public TileBaseValue(TileBase tileBase)
    {
        this.tileBase = tileBase;
    }

    public TileBase value => tileBase;

    public bool Equals(IValue<TileBase> x, IValue<TileBase> y)
    {
        return x == y;
    }

    public bool Equals(IValue<TileBase> other)
    {
        return other.value == value;
    }

    public int GetHashCode(IValue<TileBase> obj)
    {
        return obj.GetHashCode();
    }

    public override int GetHashCode()
    {
        return tileBase.GetHashCode();
    }
}

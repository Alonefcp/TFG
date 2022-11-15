using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IValue<T> : IEqualityComparer<IValue<T>>, IEquatable<IValue<T>>
{
    T value { get; }
}

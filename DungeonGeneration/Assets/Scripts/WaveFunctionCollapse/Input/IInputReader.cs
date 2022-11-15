using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputReader<T>
{
    IValue<T>[][] ReadInputToGrid();
}

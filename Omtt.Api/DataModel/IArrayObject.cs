using System;

namespace Omtt.Api.DataModel
{
    public interface IArrayObject
    {
        Object? this[Int32 index] { get; set; }
    }
}
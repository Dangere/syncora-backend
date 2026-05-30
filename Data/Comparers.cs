using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SyncoraBackend.Data;

public static class Comparers
{
    /// <summary>
    ///     A no operation value comparer for arrays of dictionaries that do not change that does not check for equality
    /// </summary>
    public static readonly ValueComparer<Dictionary<string, object>[]> arrayDictComparerNoOp = new(
        (a, b) => true,
        obj => 0,
        obj => obj
    );

    /// <summary>
    ///     A no operation value comparer for dictionaries that do not change that does not check for equality
    /// </summary>
    public static readonly ValueComparer<Dictionary<string, object>> dictComparerNoOp = new(
        (a, b) => true,
        obj => 0,
        obj => obj
    );
}
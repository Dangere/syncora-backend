using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SyncoraBackend.Data;

public static class Comparers
{

    public static readonly ValueComparer<Dictionary<string, object>[]> arrayDictComparerNoOp = new(
        (a, b) => true,
        obj => 0,
        obj => obj
    );

    public static readonly ValueComparer<Dictionary<string, object>> dictComparerNoOp = new(
        (a, b) => true,
        obj => 0,
        obj => obj
    );
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncoraBackend.Models.Common;

public record UploadSignature(string Signature, Dictionary<string, object> Parameters);

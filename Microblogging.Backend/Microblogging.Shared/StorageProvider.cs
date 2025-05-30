using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microblogging.Shared
{
    public enum StorageProvider
    {
        Local,
        AzureBlob,
        AwsS3, // not implemented yet
        GoogleCloud // not implemented yet
    }
}

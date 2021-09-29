using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            static Func<T, T> Y<T>(Func<Func<T, T>, Func<T, T>> F) => t => F(Y(F))(t);

            var files = Y<(DirectoryInfo dir, IEnumerable<DirectoryInfo> sub_dirs)>(
                GetSubDirs => dir_info =>
                {
                    var dir = dir_info.dir;
                    if (dir.FullName.Length > 70)
                        return (dir_info.dir, dir_info.sub_dirs);

                    var sub_dirs = dir.EnumerateDirectories();

                    return (dir, sub_dirs);
                });

        }

        
    }
}

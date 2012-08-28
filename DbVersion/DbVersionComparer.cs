using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbVersioning {
  public class DbVersionComparer: IComparer<DbVersion> {
    public int Compare(DbVersion a, DbVersion b) {
      if(a.MajorVersion < b.MajorVersion) return -1;
      if(a.MajorVersion > b.MajorVersion) return 1;
      // Major versions are equal
      if(a.MinorVersion < b.MinorVersion) return -1;
      if(a.MinorVersion > b.MinorVersion) return 1;
      // Major and Minor are equal
      return 0;
    }    
  }
}

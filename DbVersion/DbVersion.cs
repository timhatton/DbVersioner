using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbVersioning {
  public class DbVersion {
    public DbVersion() : this(1, 0) {}
    public DbVersion(int majorVersion, int minorVersion) {
      MajorVersion=majorVersion;
      MinorVersion=minorVersion;
    }
    public DbVersion(string version) {
      string[] parts=version.Split('.');
      MajorVersion=Int32.Parse(parts[0]);
      MinorVersion=Int32.Parse(parts[1]);
    }
    public int MajorVersion { get; set; }
    public int MinorVersion { get; set; }
    override public string ToString() {
      return string.Format("{0}.{1}", MajorVersion, MinorVersion);
    }
    public static bool operator>(DbVersion swv1, DbVersion swv2) {
      DbVersionComparer swc=new DbVersionComparer();
      return (swc.Compare(swv1, swv2)==1);
    }
    public static bool operator>=(DbVersion swv1, DbVersion swv2) {
      DbVersionComparer swc=new DbVersionComparer();
      return (swc.Compare(swv1, swv2)!=-1);
    }
    public static bool operator<(DbVersion swv1, DbVersion swv2) {
      DbVersionComparer swc=new DbVersionComparer();
      return (swc.Compare(swv1, swv2)==-1);
    }
    public static bool operator<=(DbVersion swv1, DbVersion swv2) {
      DbVersionComparer swc=new DbVersionComparer();
      return (swc.Compare(swv1, swv2)!=1);
    }
    public override bool Equals(object obj) {
      DbVersionComparer swc=new DbVersionComparer();
      return swc.Compare(this, (DbVersion)obj)==0;
    }
    public override int GetHashCode() {
      return ToString().GetHashCode();
    }
  }
}

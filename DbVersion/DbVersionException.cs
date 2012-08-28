using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DbVersioning {

  public class DbVersionException: Exception {
    public DbVersionException() : base() {}
    public DbVersionException(string message) : base(message) { }    
    public DbVersionException(string message, Exception inner): base(message, inner) {}
  }
}

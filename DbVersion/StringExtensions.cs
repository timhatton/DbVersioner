using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbVersioning {
  public static class StringExtensions {
    // Strips out tabs, returns and line feeds and replaces multiple spaces with single ones 
    // NB: a tab, return or line feed between two non-space chars will be changed to a single space
    public static string StripWhiteSpace(this string inString) {
      char lastChar=' ';

      StringBuilder sb=new StringBuilder();
      foreach(char c in inString.Trim()) {
        if(!(c=='\t' || c=='\r' || c=='\n')) {
          if(!(lastChar==' ' && c==' ')) {
            sb.Append(c);
            lastChar=c;
          }
        }
        else {
          if(lastChar!=' ') sb.Append(' ');
          lastChar=' ';
        }
      }
      return sb.ToString();
    }
  }

}

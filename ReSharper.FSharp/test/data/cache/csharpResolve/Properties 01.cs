﻿using System.Collections.Generic;
using static Module;

public class Class1
{
  public Class1()
  {
    var t = new T();
    int m = t.Prop;
    int sm = T.StaticProp;
  }
}

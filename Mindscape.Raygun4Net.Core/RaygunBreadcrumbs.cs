﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mindscape.Raygun4Net
{
  public class RaygunBreadcrumbs : IEnumerable<RaygunBreadcrumb>
  {
    public enum Level
    {
      Debug,
      Info,
      Warning,
      Error,
    }

    public enum Type
    {
      Manual
    }

    private readonly IRaygunBreadcrumbStorage _storage;

    public RaygunBreadcrumbs(IRaygunBreadcrumbStorage storage)
    {
      _storage = storage;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<RaygunBreadcrumb> GetEnumerator()
    {
      return _storage.GetEnumerator();
    }

    public void Record(string message)
    {
      Record(new RaygunBreadcrumb() { Message = message });
    }

    public void Record(RaygunBreadcrumb crumb)
    {
      if (RaygunSettings.Settings.BreadcrumbsLocationRecordingEnabled)
      {
        // 2 because it's always going to go through RaygunClient.RecordBreadcrumb or the like
        var frame = new StackFrame(2);
        var method = frame.GetMethod();

        crumb.ClassName = method.ReflectedType?.FullName;
        crumb.MethodName = method.Name;
        crumb.LineNumber = frame.GetFileLineNumber();
        if (crumb.MethodName.Contains('<'))
        {
          var unmangledName = new Regex(@"<(\w+)>").Match(crumb.MethodName).Groups[1].Value;
          crumb.MethodName = unmangledName;
        }

        if (crumb.LineNumber == 0)
          crumb.LineNumber = null;
      }

      if (ShouldRecord(crumb))
        _storage.Store(crumb);
    }

    public void Clear()
    {
      _storage.Clear();
    }

    private bool ShouldRecord(RaygunBreadcrumb crumb)
    {
      return crumb.Level >= RaygunSettings.Settings.BreadcrumbsLevel;
    }
  }
}

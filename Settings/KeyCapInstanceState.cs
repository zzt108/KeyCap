﻿////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2019 Tim Stair
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

namespace KeyCap.Settings
{
    public class KeyCapInstanceState
    {
        public bool AutoStart { get; private set; }
        public string DefaultConfigFile { get; private set; }
        public List<string> Arguments { get; private set; }

        public KeyCapInstanceState(IReadOnlyList<string> args)
        {
            Arguments = args.Select(x => x.ToLower()).ToList();
            AutoStart = IsArgPresent(CommandLineArgument.AutoStart);
            DefaultConfigFile = GetStringArg(CommandLineArgument.F);
        }

        private bool IsArgPresent(CommandLineArgument eArgument)
        {
            return Arguments.Contains("-" + eArgument.ToString().ToLower());
        }

        private string GetStringArg(CommandLineArgument eArgument)
        {
            var nIndex = Arguments.IndexOf("-" + eArgument.ToString().ToLower());
            if (nIndex == -1 || nIndex + 1 == Arguments.Count)
                return null;
            return Arguments[nIndex + 1];
        }
    }
}

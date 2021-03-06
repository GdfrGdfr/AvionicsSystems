﻿/*****************************************************************************
 * The MIT License (MIT)
 * 
 * Copyright (c) 2016-2018 MOARdV
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 * 
 ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

// Code substantially adopted from java code at https://github.com/munificent/bantam
/*
 Bantam uses the MIT License:

Copyright (c) 2011 Robert Nystrom

Permission is hereby granted, free of charge, to
any person obtaining a copy of this software and
associated documentation files (the "Software"),
to deal in the Software without restriction,
including without limitation the rights to use,
copy, modify, merge, publish, distribute,
sublicense, and/or sell copies of the Software,
and to permit persons to whom the Software is
furnished to do so, subject to the following
conditions:

The above copyright notice and this permission
notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT
WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO
EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace AvionicsSystems.CodeGen
{
    class NameExpression : Expression
    {
        public NameExpression(String name)
        {
            mName = name;

            StringBuilder sb = StringBuilderCache.Acquire();
            print(sb);
            canonicalName = sb.ToStringAndRelease();
        }

        public string CanonicalName() { return canonicalName; }

        public ExpressionIs ExpressionType() { return ExpressionIs.Name; }

        public String getName() { return mName; }

        public void print(StringBuilder builder)
        {
            builder.Append(mName);
        }

        public Type ReturnType()
        {
            // What is the right return type?
            return typeof(object);
        }

        private String mName;
        private string canonicalName;
    }

    class StringExpression : Expression
    {
        public StringExpression(String name)
        {
            mName = name.Substring(1, name.Length-2);

            StringBuilder sb = StringBuilderCache.Acquire();
            print(sb);
            canonicalName = sb.ToStringAndRelease();
        }

        public string CanonicalName() { return canonicalName; }

        public ExpressionIs ExpressionType() { return ExpressionIs.ConstantString; }
        
        public String getString() { return mName; }

        public void print(StringBuilder builder)
        {
            builder.Append('"');
            builder.Append(mName);
            builder.Append('"');
        }

        public Type ReturnType()
        {
            return typeof(string);
        }

        private String mName;
        private string canonicalName;
    }
}

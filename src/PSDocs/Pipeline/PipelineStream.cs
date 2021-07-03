// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSDocs.Data;
using System;
using System.Collections.Concurrent;
using System.Management.Automation;

namespace PSDocs.Pipeline
{
    internal sealed class PipelineStream
    {
        private readonly VisitTargetObject _Input;
        private readonly InputFileInfo[] _InputPath;
        private readonly ConcurrentQueue<TargetObject> _Queue;

        public PipelineStream(VisitTargetObject input, InputFileInfo[] inputPath)
        {
            _Input = input;
            _InputPath = inputPath;
            _Queue = new ConcurrentQueue<TargetObject>();
        }

        public int Count => _Queue.Count;

        public bool IsEmpty => _Queue.IsEmpty;

        public void Enqueue(PSObject sourceObject)
        {
            if (sourceObject == null)
                return;

            var targetObject = new TargetObject(sourceObject);
            if (_Input == null)
            {
                _Queue.Enqueue(targetObject);
                return;
            }

            // Visit the object, which may change or expand the object
            var input = _Input(targetObject);
            if (input == null)
                return;

            foreach (var item in input)
                _Queue.Enqueue(item);
        }

        public bool TryDequeue(out TargetObject targetObject)
        {
            return _Queue.TryDequeue(out targetObject);
        }

        public void Open()
        {
            if (_InputPath == null || _InputPath.Length == 0)
                return;

            // Read each file
            for (var i = 0; i < _InputPath.Length; i++)
            {
                if (_InputPath[i].IsUrl)
                {
                    Enqueue(PSObject.AsPSObject(new Uri(_InputPath[i].FullName)));
                }
                else
                {
                    Enqueue(PSObject.AsPSObject(_InputPath[i]));
                }
            }
        }
    }
}

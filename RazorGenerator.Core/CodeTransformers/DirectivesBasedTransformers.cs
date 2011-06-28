﻿using System;
using System.Collections.Generic;

namespace RazorGenerator.Core {
    public class DirectivesBasedTransformers : AggregateCodeTransformer {
        public static readonly string TypeVisibilityKey = "TypeVisibility";
        public static readonly string DisableLinePragmasKey = "DisableLinePragmas";
        private readonly List<IRazorCodeTransformer> _transformers = new List<IRazorCodeTransformer>();

        protected override IEnumerable<IRazorCodeTransformer> CodeTransformers {
            get { return _transformers; }
        }

        public override void Initialize(RazorHost razorHost, IDictionary<string, string> directives) {
            if (directives.ContainsKey(TypeVisibilityKey)) {
                _transformers.Add(new SetTypeVisibility(directives[TypeVisibilityKey]));
            }

            if (IsSwitchEnabled(directives, DisableLinePragmasKey) == true) {
                razorHost.EnableLinePragmas = false;
            }

            base.Initialize(razorHost, directives);
        }

        private static bool? IsSwitchEnabled(IDictionary<string, string> directives, string key) {
            string value;
            bool switchValue;

            if (directives.TryGetValue(key, out value) && Boolean.TryParse(value, out switchValue)) {
                return switchValue;
            }
            return null;
        }
    }
}

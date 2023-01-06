﻿namespace EasyApp.processor.components
{
    public interface IValueFetcher<TOptions>
    {
        TValue Fetch<TAttribute, TValue>(TOptions options, TValue defaultValue) where TAttribute : Attribute;

        bool Fetch<TAttribute>(TOptions options) where TAttribute : Attribute;
    }

    // TODO: Use extension instead of ValueFetcher.
    public sealed class ValueFetcher<TOptions> : IValueFetcher<TOptions>
    {
        private readonly Member[] Members;

        public ValueFetcher(Member[] members)
        {
            Members = members;
        }

        public TValue Fetch<TAttribute, TValue>(TOptions options, TValue defaultValue) where TAttribute : Attribute
        {
            var member = Members.FirstOrDefault(x => x.Attribute.GetType() == typeof(TAttribute));
            if (member == null)
            {
                return defaultValue;
            }

            var value = member.GetValue(options);
            if (value == null)
            {
                return defaultValue;
            }

            return (TValue)value;
        }

        public bool Fetch<TAttribute>(TOptions options) where TAttribute : Attribute
        {
            return Fetch<TAttribute, bool>(options, false);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core.Common.Helpers
{
    /// <summary>
    ///     Various reflection based utility methods.
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        ///     Set object properties on T using the properties collection supplied.
        ///     The properties collection is the collection of "property" to value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="properties"></param>
        /// <returns>true if all properties set, false otherwise</returns>
        public static void SetProperties<T>(T obj, IEnumerable<KeyValuePair<string, string>> properties) where T : class
        {
            // Validate
            if (obj == null)
                return;

            foreach (KeyValuePair<string, string> propVal in properties)
                SetProperty(obj, propVal.Key, propVal.Value);
        }


        /// <summary>
        ///     Set the object properties using the prop name and value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propName"> </param>
        /// <param name="propVal"> </param>
        /// <returns></returns>
        public static void SetProperty<T>(T obj, string propName, string propVal) where T : class
        {
            Guard.IsNotNull(obj, "Object containing properties to set is null");
            Guard.IsTrue(!string.IsNullOrEmpty(propName), "Property name not supplied.");

            // Remove spaces.
            if (propName != null) propName = propName.Trim();
            if (string.IsNullOrEmpty(propName))
                throw new ArgumentException("Property name is empty.");

            Type type = obj.GetType();
            PropertyInfo propertyInfo = type.GetProperty(propName);

            // Correct property with write access 
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                // Check same Type
                if (ReflectionTypeChecker.CanConvertToCorrectType(propertyInfo, propVal))
                {
                    object convertedVal = ReflectionTypeChecker.ConvertToSameType(propertyInfo, propVal);
                    propertyInfo.SetValue(obj, convertedVal, null);
                }
            }
        }


        /// <summary>
        ///     Set the property value using the string value.
        /// </summary>
        /// <param name="obj"> </param>
        /// <param name="prop"></param>
        /// <param name="propVal"> </param>
        public static void SetProperty(object obj, PropertyInfo prop, string propVal)
        {
            Guard.IsNotNull(obj, "Object containing properties to set is null");
            Guard.IsNotNull(prop, "Property not supplied.");

            // Correct property with write access 
            if (prop != null && prop.CanWrite)
            {
                // Check same Type
                if (ReflectionTypeChecker.CanConvertToCorrectType(prop, propVal))
                {
                    object convertedVal = ReflectionTypeChecker.ConvertToSameType(prop, propVal);
                    prop.SetValue(obj, convertedVal, null);
                }
            }
        }


        /// <summary>
        ///     Get the property value
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static object GetPropertyValue(object obj, string propName)
        {
            Guard.IsNotNull(obj, "Must provide object to get it's property.");
            Guard.IsTrue(!string.IsNullOrEmpty(propName), "Must provide property name to get property value.");

            if (propName != null) propName = propName.Trim();

            if (propName != null)
            {
                PropertyInfo property = obj.GetType().GetProperty(propName);
                return property == null ? null : property.GetValue(obj, null);
            }
            return string.Empty;
        }


        /// <summary>
        ///     Get all the property values.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static IList<object> GetPropertyValues(object obj, IEnumerable<string> properties)
        {
            IList<object> propertyValues = new List<object>();

            foreach (string property in properties)
            {
                PropertyInfo propInfo = obj.GetType().GetProperty(property);
                object val = propInfo.GetValue(obj, null);
                propertyValues.Add(val);
            }
            return propertyValues;
        }


        /// <summary>
        ///     Get all the properties.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propsDelimited"></param>
        /// <returns></returns>
        public static IList<PropertyInfo> GetProperties(object obj, string propsDelimited)
        {
            return GetProperties(obj.GetType(), propsDelimited.Split(','));
        }


        /// <summary>
        ///     Get all the properties.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public static IList<PropertyInfo> GetProperties(Type type, string[] props)
        {
            PropertyInfo[] allProps = type.GetProperties();
            List<PropertyInfo> propToGet = new List<PropertyInfo>();
            IDictionary<string, string> propsMap = props.ToDictionary();
            foreach (PropertyInfo prop in allProps)
            {
                if (propsMap.ContainsKey(prop.Name))
                    propToGet.Add(prop);
            }
            return propToGet;
        }


        /// <summary>
        ///     Gets the property value safely, without throwing an exception.
        ///     If an exception is caught, null is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propInfo"></param>
        /// <returns></returns>
        public static object GetPropertyValueSafely(object obj, PropertyInfo propInfo)
        {
            Guard.IsNotNull(obj, "Must provide object to get it's property.");
            if (propInfo == null) return null;

            object result;
            try
            {
                result = propInfo.GetValue(obj, null);
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }


        /// <summary>
        ///     Gets all the properties of the table.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="criteria"> </param>
        /// <returns></returns>
        public static IList<PropertyInfo> GetAllProperties(object obj, Predicate<PropertyInfo> criteria)
        {
            if (obj == null)
                return null;
            return GetProperties(obj.GetType(), criteria);
        }


        /// <summary>
        ///     Get the
        /// </summary>
        /// <param name="type"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public static IList<PropertyInfo> GetProperties(Type type, Predicate<PropertyInfo> criteria)
        {
            IList<PropertyInfo> allProperties = new List<PropertyInfo>();
            PropertyInfo[] properties = type.GetProperties();
            if (properties.Length == 0)
                return null;

            // Now check for all writeable properties.
            foreach (PropertyInfo property in properties)
            {
                // Only include writable properties and ones that are not in the exclude list.
                bool okToAdd = (criteria == null) || criteria(property);
                if (okToAdd)
                    allProperties.Add(property);
            }
            return allProperties;
        }

        /// <summary>
        ///     Gets all the properties of the object as dictionary of property names to propertyInfo.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="criteria"> </param>
        /// <returns></returns>
        public static IDictionary<string, PropertyInfo> GetPropertiesAsMap(object obj, Predicate<PropertyInfo> criteria)
        {
            IList<PropertyInfo> matchedProps = GetProperties(obj.GetType(), criteria);
            IDictionary<string, PropertyInfo> props = new Dictionary<string, PropertyInfo>();

            // Now check for all writeable properties.
            foreach (PropertyInfo prop in matchedProps)
                props.Add(prop.Name, prop);
            return props;
        }


        /// <summary>
        ///     Get the propertyInfo of the specified property name.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static PropertyInfo GetProperty(Type type, string propertyName)
        {
            IList<PropertyInfo> props = GetProperties(type, property => property.Name == propertyName);
            return props[0];
        }


        /// <summary>
        ///     Gets a list of all the writable properties of the class associated with the object.
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="criteria"></param>
        /// <remarks>
        ///     This method does not take into account, security, generics, etc.
        ///     It only checks whether or not the property can be written to.
        /// </remarks>
        /// <returns></returns>
        public static IList<PropertyInfo> GetWritableProperties(Type type, Predicate<PropertyInfo> criteria)
        {
            IList<PropertyInfo> props = GetProperties(type,
                                                      delegate(PropertyInfo property)
                                                          {
                                                              // Now determine if it can be added based on criteria.
                                                              bool okToAdd = (criteria == null)
                                                                                 ? property.CanWrite
                                                                                 : (property.CanWrite &&
                                                                                    criteria(property));
                                                              return okToAdd;
                                                          });
            return props;
        }


        /// <summary>
        ///     Invokes the method on the object provided.
        /// </summary>
        /// <param name="obj">The object containing the method to invoke</param>
        /// <param name="methodName">arguments to the method.</param>
        /// <param name="parameters"></param>
        public static object InvokeMethod(object obj, string methodName, object[] parameters)
        {
            Guard.IsNotNull(methodName, "Method name not provided.");
            Guard.IsNotNull(obj, "Can not invoke method on null object");

            methodName = methodName.Trim();

            // Validate.
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentException("Method name not provided.");

            MethodInfo method = obj.GetType().GetMethod(methodName);
            object output = method.Invoke(obj, parameters);
            return output;
        }
    }
}
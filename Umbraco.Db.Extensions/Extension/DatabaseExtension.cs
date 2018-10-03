using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Persistence;

namespace Umbraco.Db.Extensions
{
    public static class DatabaseExtension
    {
        private static void Encrypt<T>(T poco) where T : IEncrypt
        {
            var dto = ValidateEncryptPoco(poco);
            if (dto.Exception != null)
            {
                throw dto.Exception;
            }
            var properties = poco.GetType().GetProperties();
            foreach (var property in properties)
            {
                Encrypt(poco, property);
            }
        }

        private static void Encrypt<T>(T poco, PropertyInfo property) where T : IEncrypt
        {
            var propertyDto = ValidateEncryptPocoProperty(poco, property);
            if (propertyDto.Exception != null)
            {
                throw propertyDto.Exception;
            }

            if (propertyDto.Attribute != null)
            {
                var value = property.GetValue(poco);
                if (property.PropertyType != typeof(string))
                {
                    throw new Exception($"Data Type of the {property.Name} must be a string");
                }

                var encryptValue = value?.ToString() ?? "";
                if (encryptValue != "")
                {
                    var aes = Aes.Encrypt(encryptValue, poco.EKey);
                    property.SetValue(poco, aes.Encrypted);
                    propertyDto.KeyProperty.SetValue(poco, aes.IV);
                }

            }
        }

        private static void Decrypt<T>(T poco) where T : IEncrypt
        {
            var properties = poco.GetType().GetProperties();
            foreach (var property in properties)
            {
                var dto = ValidateEncryptPocoProperty(poco, property);
                if (dto.Exception != null)
                {
                    throw dto.Exception;
                }

                if (dto.Attribute != null)
                {
                    var encrypted = (string)property.GetValue(poco);
                    var iv = (string)dto.KeyProperty.GetValue(poco);
                    if (!string.IsNullOrEmpty(encrypted) && !string.IsNullOrEmpty(iv))
                    {
                        var aesResult = new EncryptionResult(encrypted, iv, poco.EKey);
                        property.SetValue(poco, Aes.Decrypt(aesResult));
                    }
                }
            }
        }

        private static EncryptDto ValidateEncryptPoco<T>(T poco)
        {
            var dto = new EncryptDto { Poco = (IEncrypt)poco };

            if(dto.Poco == null)
            {
                dto.Exception = new Exception("Class must inherit from IEncrypt");
            }
            else if(dto.Poco.EKey == null || !dto.Poco.EKey.Any())
            {
                dto.Exception = new Exception("EKey cannot be null or empty");
            }

            return dto;
        }

        private static EncryptPropertyDto ValidateEncryptPocoProperty<T>(T poco, PropertyInfo property)
        {
            var dto = new EncryptPropertyDto { Attribute = property.GetCustomAttribute<EncryptAttribute>() };

            if(dto.Attribute != null)
            {
                if (string.IsNullOrEmpty(dto.Attribute.KeyPropertyName))
                {
                    dto.Exception = new Exception("Property: KeyPropertyName cannot be null or empty");
                }
                else
                {
                    var pocoType = poco.GetType();
                    dto.KeyProperty = pocoType.GetProperty(dto.Attribute.KeyPropertyName);
                    if (dto.KeyProperty == null)
                    {
                        dto.Exception = new Exception($"Property {dto.Attribute.KeyPropertyName} doesn't exist in the class {pocoType.Name}");
                    }
                    else if (dto.KeyProperty.PropertyType != typeof(string))
                    {
                        dto.Exception = new Exception($"Data Type of {dto.KeyProperty.Name} must be a string");
                    }
                }
            }

            return dto;
        }

        public static List<T> Fetch<T>(this Database db, string sql, bool useDecryption) where T : IEncrypt
        {
            var pocos = db.Fetch<T>(sql);
            if(useDecryption)
            {
                pocos.ForEach(x => Decrypt(x));
            }            
            return pocos;
        }

        public static T First<T>(this Database db, string sql, bool useDecryption) where T : IEncrypt
        {
            var poco = db.First<T>(sql);
            if(useDecryption)
            {
                Decrypt(poco);
            }
            return poco;
        }

        public static T FirstOrDefault<T>(this Database db, string sql, bool useDecryption) where T : IEncrypt
        {
            var poco = db.FirstOrDefault<T>(sql);
            if(useDecryption && poco != null)
            {
                Decrypt(poco);
            }
            return poco;
        }

        public static T Insert<T>(this Database db, T poco, bool useEncryption) where T : IEncrypt
        {
            if(useEncryption)
            {
                Encrypt(poco);
            }
            db.Insert(poco);
            return poco;
        }

        public static void Save<T>(this Database db, T poco, bool useEncryption) where T : IEncrypt
        {
            if(useEncryption)
            {
                Encrypt(poco);
            }
            db.Save(poco);
        }
    }

    internal class EncryptDto
    {
        public Exception Exception { get; set; }
        public IEncrypt Poco { get; set; }
    }

    internal class EncryptPropertyDto
    {
        public EncryptAttribute Attribute { get; set; }
        public Exception Exception { get; set; }
        public PropertyInfo KeyProperty { get; set; }
    }
}

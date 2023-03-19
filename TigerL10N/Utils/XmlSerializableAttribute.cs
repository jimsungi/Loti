using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

using System.Collections.Generic;

using System.Text;

using System.Xml.Serialization;

using System.Runtime.Serialization;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.VisualBasic;
using System.IO;
using System.Runtime.Intrinsics.X86;

namespace TigerL10N.Utils
{




    /// <summary>

    /// Xml serializable dictionary object.

    /// </summary>

    /// <typeparam name="TKey">Key type</typeparam>

    /// <typeparam name="TValue">Object type</typeparam>

    [SerializableAttribute]

    public class XmlSerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {

        #region Constructors.

        /// <summary>

        /// Initializes a new instance of the <see cref="XmlSerializableDictionary&lt;TKey, TValue&gt;"/> class.

        /// </summary>

        public XmlSerializableDictionary()

            : base()

        {

            // default constructor.

        }

        /// <summary>

        /// Initializes a new instance of the <see cref="XmlSerializableDictionary&lt;TKey, TValue&gt;"/> class.

        /// </summary>

        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> object containing the  

        //information required to serialize the<see cref="T:System.Collections.Generic.Dictionary`2"></see>.</param>

        ///// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"></see> structure

        //containing the source and destination of the serialized stream associated with the

        //<see  cref= "T:System.Collections.Generic.Dictionary`2" ></ see >.</ param >


        protected XmlSerializableDictionary(SerializationInfo info, StreamingContext context)

            : base(info, context)

        {

        }

        #endregion



        #region Constants.

        private const string ITEM = "item";

        private const string KEY = "key";

        private const string VALUE = "value";

        #endregion

        #region IXmlSerializable Members

        /// <summary>

        /// This property is reserved, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"></see> to

        //the class instead.

        /// </summary>

        /// <returns>

        /// An <see cref="T:System.Xml.Schema.XmlSchema"></see> that describes the XML representation of the object that is

        //produced by the<see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"></see> method

        //and consumed by the<see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"></see>

        //method.

        /// </returns>

        public System.Xml.Schema.XmlSchema GetSchema()

        {

            return null;

        }

        /// <summary>

        /// Generates an object from its XML representation.

        /// </summary>

        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"></see> stream from which the object is

        //deserialized.</param>

        public void ReadXml(System.Xml.XmlReader reader)

        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();

            if (wasEmpty)

                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {

                reader.ReadStartElement(ITEM);

                reader.ReadStartElement(KEY);
                var k = keySerializer.Deserialize(reader);
                TKey? key = default(TKey);
                if(k!=null)
                {
                    key = (TKey?)k;
                }
                if (key != null)
                {
                    reader.ReadEndElement();

                    reader.ReadStartElement(VALUE);
                    TValue? value = default(TValue);
                    var v = valueSerializer.Deserialize(reader);
                    value = (TValue?)v;
                    if (value != null)
                    {
                        reader.ReadEndElement();
                        this.Add(key, value);
                        reader.ReadEndElement();
                        reader.MoveToContent();
                    }
                }

            }

            reader.ReadEndElement();

        }

        /// <summary>

        /// Converts an object into its XML representation.

        /// </summary>

        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"></see> stream to which the object is

        //serialized.</param>

        public void WriteXml(System.Xml.XmlWriter writer)

        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)

            {

                writer.WriteStartElement(ITEM);

                writer.WriteStartElement(KEY);

                keySerializer.Serialize(writer, key);

                writer.WriteEndElement();

                writer.WriteStartElement(VALUE);

                TValue value = this[key];

                valueSerializer.Serialize(writer, value);

                writer.WriteEndElement();

                writer.WriteEndElement();

            }

        }

        #endregion
    }
}


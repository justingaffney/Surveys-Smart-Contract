using Neo.SmartContract.Framework;

namespace Survey.Contract
{
    internal static class EncodingHelper
    {
        #region Survey Metadata Encoding and Setter Methods

        /// <summary>
        ///     Encoding scheme:
        ///     IsClosed (1 byte)|EndBlock (4 bytes)|Description (variable length)
        /// </summary>
        /// <returns>Encoded survey metadata.</returns>
        internal static byte[] EncodeSurveyMetadata(bool isClosed, uint endBlock, string description)
        {
            if (description == null) description = "";


            // Encode IsClosed
            var encodedMetadata = new byte[1];
            if (isClosed)
            {
                encodedMetadata[0] = 1;
            }
            else
            {
                encodedMetadata[0] = 0;
            }


            // Encode EndBlock
            encodedMetadata = encodedMetadata.Concat(endBlock.AsByteArray());


            // Encode Description
            encodedMetadata = encodedMetadata.Concat(description.AsByteArray());


            // Encode successful
            return encodedMetadata;
        }

        internal static byte[] SetSurveyIsClosed(byte[] encodedMetadata)
        {
            if (encodedMetadata.Length == 0) return encodedMetadata;

            encodedMetadata[0] = 1;

            return encodedMetadata;
        }

        #endregion Survey Metadata Encoding and Setter Methods

        #region Survey Metadata Decoding Getter Methods

        internal static bool GetSurveyIsClosed(byte[] encodedMetadata)
        {
            if (encodedMetadata.Length == 0) return false;

            var isClosedByte = encodedMetadata[0];

            var isClosed = isClosedByte > 0;

            return isClosed;
        }

        internal static uint GetSurveyEndBlock(byte[] encodedMetadata)
        {
            if (encodedMetadata.Length < 5) return 0;

            var endBlockBytes = encodedMetadata.Range(1, 4);

            var endBlock = AsUInt(endBlockBytes);

            return endBlock;
        }

        internal static string GetSurveyDescription(byte[] encodedMetadata)
        {
            if (encodedMetadata.Length <= 5) return "";

            var descriptionBytes = encodedMetadata.Range(5, encodedMetadata.Length - 5);

            var description = descriptionBytes.AsString();

            return description;
        }

        #endregion Survey Metadata Decoding Getter Methods
                
        /// <summary>
        /// Little-endian
        /// </summary>
        internal static uint AsUInt(byte[] source)
        {
            uint value = 0;

            if (source.Length != 4) return value;

            value |= ((uint)source[0]);
            value |= (((uint)source[1]) << 8);
            value |= (((uint)source[2]) << 16);
            value |= (((uint)source[3]) << 24);

            return value;
        }

        /// <summary>
        /// Little-endian
        /// </summary>
        internal static byte[] AsByteArray(this uint source)
        {
            var firstByte = (byte)(source & 0xFF);
            var secondByte = (byte)((source >> 8) & 0xFF);
            var thirdByte = (byte)((source >> 16) & 0xFF);
            var fourthByte = (byte)((source >> 24) & 0xFF);

            return new byte[]
            {
                firstByte,
                secondByte,
                thirdByte,
                fourthByte
            };
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AntShares.SmartContract
{
    internal static class PollEncoder
    {
        internal static byte[] Encode(bool isOpen, byte[] poller, string question, Dictionary<byte, string> options, Dictionary<byte[], byte> responses = null)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    try
                    {
                        // Encode header
                        EncodeHeader(writer, poller, isOpen, question, options.Count);
                        

                        // Encode question
                        var questionBytes = Encoding.UTF8.GetBytes(question);

                        // Write encoded question
                        writer.Write(questionBytes);


                        // Write poller address
                        writer.Write(poller);


                        /* Options */

                        // Write options
                        foreach (var option in options)
                        {
                            // Encode option
                            var optionBytes = Encoding.UTF8.GetBytes(option.Value);

                            // Write option identifier
                            writer.Write(option.Key);

                            // Write option length
                            writer.Write(optionBytes.Length);

                            // Write option
                            writer.Write(optionBytes);
                        }


                        /* Responses */
                        if (responses != null && responses.Count > 0)
                        {
                            // Poll does not have any responses
                            writer.Write(0);
                        }
                        else
                        {
                            // Write number of responses
                            writer.Write(responses.Count);

                            // Write responses
                            foreach (var response in responses)
                            {
                                EncodeResponse(writer, response.Key, response.Value);
                            }
                        }

                        // Poll encoded successfully
                        writer.Flush();
                        return stream.ToArray();
                    }
                    catch
                    {
                        // Swallow exception
                        return null;
                    }
                }
            }
        }

        internal static bool Decode(byte[] encodedPoll, out bool isOpen, out byte[] poller, out string question, out Dictionary<byte, string> options, out Dictionary<byte[], byte> responses)
        {
            using (var stream = new MemoryStream())
            {
                using (var reader = new BinaryReader(stream))
                {
                    try
                    {
                        // Decode header
                        int pollerAddressLength;
                        int questionLength;
                        int optionsCount;
                        DecodeHeader(reader, out pollerAddressLength, out questionLength, out optionsCount, out isOpen);
                                                

                        // Read encoded question
                        var questionBytes = reader.ReadBytes(questionLength);

                        // Decode question
                        question = Encoding.UTF8.GetString(questionBytes);


                        // Read poller address
                        poller = reader.ReadBytes(pollerAddressLength);


                        /* Options */
                        options = new Dictionary<byte, string>();

                        // Read options
                        for (var i = 0; i < optionsCount; i++)
                        {
                            // Read option identifier
                            var optionId = reader.ReadByte();

                            // Read option length
                            var optionLength = reader.ReadInt32();

                            // Read option
                            var optionBytes = reader.ReadBytes(optionLength);

                            var option = Encoding.UTF8.GetString(optionBytes);

                            options.Add(optionId, option);
                        }


                        /* Responses */
                        responses = new Dictionary<byte[], byte>();

                        // Read number of responses
                        var responsesCount = reader.ReadInt32();

                        // Read responses
                        for (var i = 0; i < responsesCount; i++)
                        {
                            byte[] responderAddress;
                            byte response;
                            DecodeResponse(reader, out responderAddress, out response);

                            responses.Add(responderAddress, response);
                        }

                        // Poll decoded successfully
                        return true;
                    }
                    catch
                    {
                        // Swallow exception
                        isOpen = false;
                        poller = null;
                        question = null;
                        options = null;
                        responses = null;

                        return false;
                    }
                }
            }
        }

        private static void EncodeHeader(BinaryWriter writer, byte[] poller, bool isOpen, string question, int optionCount)
        {
            // Write question length
            writer.Write(Encoding.UTF8.GetByteCount(question));

            // Write poller address length
            writer.Write(poller.Length);

            // Write number of options
            writer.Write(optionCount);

            // Write is open property
            writer.Write(isOpen);
        }

        private static void DecodeHeader(BinaryReader reader, out int pollerAddressLength, out int questionLength, out int optionsCount, out bool isOpen)
        {
            // Read question length
            questionLength = reader.ReadInt32();

            // Read poller address length
            pollerAddressLength = reader.ReadInt32();

            // Read number of options
            optionsCount = reader.ReadInt32();

            // Read is open property
            isOpen = reader.ReadBoolean();
        }

        private static void EncodeResponse(BinaryWriter writer, byte[] responderAddress, byte response)
        {
            // Write responder address length
            writer.Write(responderAddress.Length);

            // Write responder address
            writer.Write(responderAddress);

            // Write response
            writer.Write(response);
        }

        private static void DecodeResponse(BinaryReader reader, out byte[] responderAddress, out byte response)
        {
            // Read responder address length
            var responderAddressLength = reader.ReadInt32();

            // Read responder address
            responderAddress = reader.ReadBytes(responderAddressLength);

            // Read response
            response = reader.ReadByte();
        }



        internal static byte[] AddResponse(byte[] encodedPoll, byte[] responder, byte response)
        {
            using (var stream = new MemoryStream(encodedPoll))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    EncodeResponse(writer, responder, response);

                    stream.Flush();
                    return stream.ToArray();
                }
            }
        }

        internal static string DecodeQuestion(byte[] encodedPoll)
        {
            using (var stream = new MemoryStream())
            {
                using (var reader = new BinaryReader(stream))
                {
                    try
                    {
                        // Read question length
                        var questionLength = reader.ReadInt32();

                        // Ignore poller address length
                        reader.ReadInt32();

                        // Ignore number of options
                        reader.ReadInt32();

                        // Ignore is open property
                        reader.ReadBoolean();


                        // Read encoded question
                        var questionBytes = reader.ReadBytes(questionLength);

                        // Decode question
                        var question = Encoding.UTF8.GetString(questionBytes);

                        // Poll question decoded successfully
                        return question;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }
    }
}
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
                        /* Poll status */
                        // Write is open property
                        writer.Write(isOpen);
                        

                        /* Poller address */
                        // Write address length
                        writer.Write(poller.Length);

                        // Write address
                        writer.Write(poller);


                        /* Question */
                        // Encode question
                        var questionBytes = Encoding.UTF8.GetBytes(question);

                        // Write question length
                        writer.Write(questionBytes.Length);

                        // Write question
                        writer.Write(questionBytes);


                        /* Options */

                        // Write number of options
                        writer.Write(options.Count);

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
                                // Write responder address length
                                writer.Write(response.Key.Length);

                                // Write responder address
                                writer.Write(response.Key);

                                // Write response
                                writer.Write(response.Value);
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
                        /* Poll status */
                        // Read is open property
                        isOpen = reader.ReadBoolean();


                        /* Poller address */
                        // Read address length
                        var pollerAddressLength = reader.ReadInt32();

                        // Read address
                        poller = reader.ReadBytes(pollerAddressLength);


                        /* Question */
                        // Read question length
                        var questionLength = reader.ReadInt32();

                        // Read encoded question
                        var questionBytes = reader.ReadBytes(questionLength);

                        // Decode question
                        question = Encoding.UTF8.GetString(questionBytes);


                        /* Options */
                        options = new Dictionary<byte, string>();

                        // Read number of options
                        var optionsCount = reader.ReadInt32();

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
                            // Read responder address length
                            var responderAddressLength = reader.ReadInt32();

                            // Read responder address
                            var responderAddress = reader.ReadBytes(responderAddressLength);

                            // Read response
                            var response = reader.ReadByte();

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
    }
}
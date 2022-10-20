using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Threading;

namespace TAS.Shared;

public abstract class CommunicationBase {
    private const int BufferSize = 0x100000;

    // ReSharper disable once MemberCanBePrivate.Global
    protected const int Timeout = 16;

    protected readonly MemoryMappedFile gameToStudioMMF;
    protected readonly MemoryMappedFile studioToGameMMF;

    private int failedWrites;
    private int lastSignature;

    protected Action PendingWrite;
    private int timeoutCount;
    private bool waiting;

    protected abstract MemoryMappedFile writeMMF { get; }
    protected abstract MemoryMappedFile readMMF { get; }

    protected CommunicationBase(string gameName) {
        studioToGameMMF = MemoryMappedFile.CreateOrOpen(gameName + "_studioToGame", BufferSize);
        gameToStudioMMF = MemoryMappedFile.CreateOrOpen(gameName + "_gameToStudio", BufferSize);
    }

    public static bool Initialized { get; protected set; }

    ~CommunicationBase() {
        writeMMF.Dispose();
        readMMF.Dispose();
    }

    protected void UpdateLoop() {
        while (true) {
            EstablishConnectionLoop();
            try {
                while (true) {
                    Message? message = ReadMessage();

                    if (message != null) {
                        ReadData((Message) message);
                        waiting = false;
                    }

                    Thread.Sleep(Timeout);

                    if (!NeedsToWait()) {
                        PendingWrite?.Invoke();
                        PendingWrite = null;
                    }
                }
            }
            //For this to work all writes must occur in this thread
            catch (NeedsResetException e) {
                ForceReset(e);
            }
        }
    }

    protected virtual bool NeedsToWait() => waiting;

    private bool IsHighPriority(MessageID id) =>
        Attribute.IsDefined(typeof(MessageID).GetField(Enum.GetName(typeof(MessageID), id)), typeof(HighPriorityAttribute));

    protected Message? ReadMessage() {
        MessageID id = default;
        int signature;
        int size;
        byte[] data;

        using (MemoryMappedViewStream stream = readMMF.CreateViewStream()) {
            BinaryReader reader = new(stream);
            BinaryWriter writer = new(stream);

            id = (MessageID) reader.ReadByte();
            if (id == MessageID.Default) {
                return null;
            }

            //Make sure the message came from the other side
            signature = reader.ReadInt32();
            if (signature == lastSignature) {
                return null;
            }

            size = reader.ReadInt32();
            data = reader.ReadBytes(size);

            //Overwriting the first byte ensures that the data will only be read once
            stream.Position = 0;
            writer.Write((byte) 0);
        }


        Message message = new(id, data);
        return message;
    }

    protected Message ReadMessageGuaranteed() {
        Log($"{this} forcing read");
        int failedReads = 0;
        while (true) {
            Message? message = ReadMessage();
            if (message != null) {
                return (Message) message;
            }

            if ( /*Initialized &&*/ ++failedReads > 100) {
                throw new NeedsResetException("Read timed out");
            }

            Thread.Sleep(Timeout);
        }
    }

    protected bool WriteMessage(Message message) {
        using (MemoryMappedViewStream stream = writeMMF.CreateViewStream()) {
            BinaryReader reader = new(stream);
            BinaryWriter writer = new(stream);

            //Check that there isn't a message waiting to be read
            byte firstByte = reader.ReadByte();
            if (firstByte != 0 && (!IsHighPriority(message.Id) || IsHighPriority((MessageID) firstByte))) {
                if ( /*Initialized &&*/ ++failedWrites > 100) {
                    throw new NeedsResetException("Write timed out");
                }

                return false;
            }

            stream.Position = 0;
            writer.Write(message.GetBytes());
        }

        lastSignature = Message.Signature;
        failedWrites = 0;
        return true;
    }

    protected void WriteMessageGuaranteed(Message message) {
        while (true) {
            if (WriteMessage(message)) {
                break;
            }

            Thread.Sleep(Timeout);
        }
    }

    protected void ForceReset(NeedsResetException e) {
        Initialized = false;
        waiting = false;
        failedWrites = 0;
        PendingWrite = null;
        timeoutCount++;
        Log($"Exception thrown - {e.Message}");
        //Ensure the first byte of the mmf is reset
        using (MemoryMappedViewStream stream = writeMMF.CreateViewStream()) {
            BinaryWriter writer = new(stream);
            writer.Write((byte) 0);
        }

        WriteReset();
        Thread.Sleep(Timeout * 2);
    }

    // Only needs to be used on the game end, as game will detect disconnects much faster
    protected virtual void WriteReset() {
        using (MemoryMappedViewStream stream = writeMMF.CreateViewStream()) {
            BinaryWriter writer = new(stream);
            Message reset = new(MessageID.Reset, new byte[0]);
            writer.Write(reset.GetBytes());
        }
    }

    public void WriteWait() {
        PendingWrite = () => WriteMessageGuaranteed(new Message(MessageID.Wait, new byte[0]));
    }

    protected void ProcessWait() {
        waiting = true;
    }

    protected virtual void ReadData(Message message) { }

    private void EstablishConnectionLoop() {
        while (true) {
            try {
                EstablishConnection();
                timeoutCount = 0;
                break;
            } catch (NeedsResetException e) {
                ForceReset(e);
            }
        }
    }

    protected virtual void EstablishConnection() { }

    public override string ToString() {
        string location = Assembly.GetExecutingAssembly().GetName().Name;
        return $"CommunicationBase Location @ {location}";
    }

    protected void Log(string s) {
        if (timeoutCount <= 5) {
            Console.WriteLine(s);
            System.Diagnostics.Trace.WriteLine(s);
        }
    }
    // This is literally the first thing I have ever written with threading
    // Apologies in advance to anyone else working on this

    // ReSharper disable once StructCanBeMadeReadOnly
    public struct Message {
        public MessageID Id { get; }
        public byte[] Data { get; }
        public int Length => Data.Length;

        public static readonly int Signature = Thread.CurrentThread.GetHashCode();
        private const int HeaderLength = 9;

        public Message(MessageID id, byte[] data) {
            Id = id;
            Data = data;
        }

        public byte[] GetBytes() {
            byte[] bytes = new byte[Length + HeaderLength];
            bytes[0] = (byte) Id;
            Buffer.BlockCopy(BitConverter.GetBytes(Signature), 0, bytes, 1, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Length), 0, bytes, 5, 4);
            Buffer.BlockCopy(Data, 0, bytes, HeaderLength, Length);
            return bytes;
        }
    }

    protected class NeedsResetException : Exception {
        public NeedsResetException() { }
        public NeedsResetException(string message) : base(message) { }
    }
}
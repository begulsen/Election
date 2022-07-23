using System;

namespace Election.Exceptions
{
    public class ApiException
    {
        
        public static class BadRequestExceptionCodes
        {
            public const ushort ElectionInfoExist = 4000;
            public const ushort ValueCannotBeNullOrEmptyException = 4001;
            public const ushort MediaMaxSizeNotValidException = 4002;
            public const ushort MediaMinSizeNotValidException = 4003;
            public const ushort MediaExtensionNotValidException = 4004;
            public const ushort ElectionInfoNotExist = 4005;
            public const ushort ElectionImageNotExist = 4006;
        }

        public abstract class BadRequestException : Exception
        {
            protected BadRequestException(string message) : base(message) { }
            public abstract ushort Code { get; }
        }
        
        public abstract class ConflictException : Exception
        {
            protected ConflictException(string message) : base(message) { }
            public abstract ushort Code { get; }
        }
        
        public abstract class NotFoundException : Exception
        {
            protected NotFoundException(string message) : base(message) { }
            public abstract ushort Code { get; }
        }
        
        public class ValueCannotBeNullOrEmptyException : BadRequestException
        {
            public ValueCannotBeNullOrEmptyException(string value) : base(value + " cannot be null or empty!") { }
            public override ushort Code => BadRequestExceptionCodes.ValueCannotBeNullOrEmptyException;
        }
        
        public class ElectionInfoExist : ConflictException
        {
            public ElectionInfoExist(string name) : base("Election: " + name + " is already exist!") { }
            public override ushort Code => BadRequestExceptionCodes.ElectionInfoExist;
        }
        
        public class ElectionInfoNotExist : ConflictException
        {
            public ElectionInfoNotExist(string name) : base("Election: " + name + " is not exist!") { }
            public override ushort Code => BadRequestExceptionCodes.ElectionInfoNotExist;
        }

        public class ElectionImageNotExist : ConflictException
        {
            public ElectionImageNotExist() : base("Election Image: is not exist!") { }
            public override ushort Code => BadRequestExceptionCodes.ElectionImageNotExist;
        }

        public class MediaMaxSizeNotValidException : BadRequestException
        {
            public MediaMaxSizeNotValidException() : base("Media size is not valid! Maximum 10MB is allowed.") { }
            public override ushort Code => BadRequestExceptionCodes.MediaMaxSizeNotValidException;
        }
        
        public class MediaMinSizeNotValidException : BadRequestException
        {
            public MediaMinSizeNotValidException() : base("Media size is not valid! Minimum 10KB is allowed.") { }
            public override ushort Code => BadRequestExceptionCodes.MediaMinSizeNotValidException;
        }

        public class MediaExtensionNotValidException : BadRequestException
        {
            public MediaExtensionNotValidException() : base("Media extension is not valid! Jpeg and png is allowed.") { }
            public override ushort Code => BadRequestExceptionCodes.MediaExtensionNotValidException;
        }
    }
}
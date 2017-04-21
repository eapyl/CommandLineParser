using System;
using System.IO;

namespace CommandLineParser.Arguments
{
	/// <summary>
	/// Value of the argument is an existing file (input file) or 
	/// a file that can be created (output file).
	/// </summary>
	public class FileArgument: CertifiedValueArgument<FileInfo>
	{
        #region constructor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FileArgument"/> class.
        /// </summary>
        /// <param name="shortName">The short name.</param>
        /// <param name="longName">The long name.</param>
        /// <param name="description">The description.</param>
        public FileArgument(char? shortName = null, string longName = null, string description = null) : base(shortName, longName, description) { }

        #endregion

        #region property 

        /// <summary>
        /// Gets or sets a value indicating whether the file must
        /// already exists in the file system (input file) or not (output file).
        /// Default is true.
        /// </summary>
        public bool FileMustExist { get; set; } = true;

        /// <summary>
        /// FileInfo for the file passed as argument.
        /// </summary>
        public FileInfo FileInfo => Value;

        #endregion

        #region methods

        /// <summary>
        /// Returns stream of the input file ready for reading. Available only 
        /// when <see cref="FileMustExist"/> is set to true.
        /// </summary>
        /// <returns></returns>
        public FileStream OpenFileRead()
		{
			if (!FileMustExist)
			{
				throw new ArgumentException(Messages.EXC_FILE_MUST_EXIST);
			}
			return Value.OpenRead();
		}

        /// <summary>
        /// Returns stream of the output file ready for writing. 
        /// </summary>
        /// <returns></returns>
        public FileStream OpenFileWrite()
		{
			return Value.OpenWrite();
		}

		/// <summary>
		/// Converts <paramref name="stringValue"/> to <see cref="FileInfo"/>
		/// </summary>
		/// <param name="stringValue">string representing the value</param>
		/// <returns>value as <see cref="FileInfo"/></returns>
		public override FileInfo Convert(string stringValue)
		{
			return new FileInfo(stringValue);
		}

        /// <summary>
        /// Checks whether file exists in the file system
        /// </summary>
        /// <param name="value">value to certify - file path</param>
        protected override void Certify(FileInfo value)
		{
			if (FileMustExist && !value.Exists)
			{
				throw new FileNotFoundException(string.Format(Messages.EXC_FILE_NOT_FOUND, value.Name));
			}
		}

        #endregion
    }

	/// <summary>
	/// <para>
	/// Attribute for declaring a class' field a <see cref="FileArgumentAttribute"/> and 
	/// thus binding a field's value to a certain command line switch argument.
	/// </para>
	/// <para>
	/// Instead of creating an argument explicitly, you can assign a class' field an argument
	/// attribute and let the CommandLineParse take care of binding the attribute to the field.
	/// </para>
	/// </summary>
	/// <remarks>Appliable to fields and properties (public).</remarks>
	/// <remarks>Use <see cref="CommandLineParser.ExtractArgumentAttributes"/> for each object 
	/// you where you have delcared argument attributes.</remarks>	
	public sealed class FileArgumentAttribute : ArgumentAttribute
	{
		/// <summary>
		/// Creates new instance of FileArgumentAttribute. FileArgumentAttribute
		/// uses underlying <see cref="FileArgument"/>.
		/// </summary>
		public FileArgumentAttribute() : base(typeof(FileArgument)) { }

		/// <summary>
		/// Gets or sets a value indicating whether the file must
		/// already exists in the file system (input file) or not (output file).
		/// Default is true.
		/// </summary>
		public bool FileMustExist
		{
			get { return ((FileArgument)Argument).FileMustExist; }
			set { ((FileArgument)Argument).FileMustExist = value; }
		}

        /// <summary>
        /// Default value
        /// </summary>
        public FileInfo DefaultValue
        {
            get
            {
                return ((FileArgument) Argument).DefaultValue;
            }
            set
            {
                ((FileArgument)Argument).DefaultValue = value;
            }
        }
	}
}
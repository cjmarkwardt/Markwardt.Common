namespace Markwardt;

[ServiceType<string>]
public class ApplicationDataFolderPathTag : CreatorTag<ApplicationDataFolderPathTag.Creator>
{
    public class Creator : IServiceCreator
    {
        [Inject<ApplicationDataRootFolderPathTag>]
        public required string Root { get; init; }

        [Inject<ApplicationNameTag>]
        public required string ApplicationName { get; init; }

        public object Create()
            => Path.Combine(Root, ApplicationName);
    }
}
namespace Markwardt;

public class AutoHandler() : CompositeServiceHandler([new TagServiceHandler(), new AttributeServiceHandler(), new FactoryServiceHandler(), new DefaultServiceHandler()]);
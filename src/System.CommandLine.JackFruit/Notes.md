## Discussion on TypeBinder

* There are cases where this could be a generic class, perhaps worth a generic layer here
* There needs to be a constructor that doesn't require the type have a constructor
* Caching the constructor and properties may not justify the stateful class since it's highly unlikely operations will happen more than once
* Is the service provider BYOB? This might help a problem Will sees using DI to create the instances for constructor injection
* IsMatch uses "FromKebabCase". We should discuss canonical comparisons, perhaps against kebabs as there is not fidelitiy in round trip
* It looks like we prefer Options to Arguments, and I think we should consider the reverse
* I think we should remove the option creation from method and type binders. I think this should be a separate class, which can be extended in app models
* Binders may also need to be extended in app models, but I have not yet needed to do that. 
* We're effectively using two approaches to internal types - From service as we set properties, and I assume parameters, but via hard coding wheh we build. Asymetry here will bite us
* 


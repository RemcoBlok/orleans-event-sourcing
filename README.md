# orleans-event-sourcing

This is an example of a CQRS + event sourcing implementation using Orleans.

I have actors that process commands and produce events and I have separate actors that project those events to projections and deal with the queries retrieving the projections.

The command actors stream the events. The projector actors subscribe to those streams. Multiple projector actors can subscribe to the same stream to project the events in different ways to different projections.

The commands require the single-threaded execution of actors to avoid concurrency violations. The projectors also require the single-threaded execution of actors to project the events in the correct sequence and to avoid concurrency violations. The queries on the other hand can retrieve the current state of the projections in parallel without blocking the projector from projecting new events through the use of the AlwaysInterleaveAttribute (and by being separate actors also without blocking the command actors). 

The Orleans.EventSourcing.**LogStorage**.LogConsistencyProvider, while storing the individual events (as opposed to the Orleans.EventSourcing.**StateStorage**.LogConsistencyProvider that stores the state but not the individual events), persists the individual events in a single row in Azure Table Storage that is constantly updated and could grow very large in size with many events. Event storage should be append-only. So I ended up using Orleans.EventSourcing.**CustomStorage**.LogConsistencyProvider with my own implementation that persists the indivual events to separate rows in Azure Table Storage. I have not implementing snapshots though.

The example uses the Azure Queue stream provider for durable queues. Azure Queues are not rewindable though. If at any time in the future I want to change my projections I want to rewind the streams to the very first event to recreate my projections. I could use Azure Event Hubs that are rewindable up to a certain expiration time of the events. What I would really like is to use the Azure **Table** Storage to allow me to rewind streams to the very first event without any expiration time of the events. 

To make this possible I have a CategoryEventsProjector that stores all events from all actors in a new stream with its own versioning of all the events (inspired by the by_category projection in EvenstoreDB, see https://developers.eventstore.com/server/v22.10/projections.html#by-category). I could build a pulling agent that pulls all events out of these streams and publishes them again to Orleans Streaming to allow all projectors to recreate their projections from the very first event. The category events stream is needed because I don't know how many streams of events were created and what their stream id may be. The category events stream contains all events from all actor instances.

To create a cursor over Azure Table Storage I need the RowKey to be sortable. The RowKey is a string and is filled with the integer Version (sequence number) of an event. So to make the string RowKey sortable I need to format the integer Version using version.ToString("D19", CultureInfo.InvariantCulture).

The CategoryEvents stream could be the only stream that the command actors publish their events to. All other streams could be published to from the CategoryEventsProjector (or from the pulling agent mentioned above). This would be a potential bottleneck though. So for now the command actors publish their events to all possible streams that the different projectors require. Assuming the publishing to the streams is fast (faster than the command actors calling all projector actors directly), this should not hold up the command actors too much.


\connect box

CREATE TABLE "partition-topic-0"
(
    boxid bigint PRIMARY KEY,
    packageid bigint NOT NULL
);

CREATE INDEX "idx_partition-topic-0"
ON "partition-topic-0"(packageid);

CREATE TABLE "partition-topic-0-offset"
(
    topic_offset bigint PRIMARY KEY
);

CREATE TABLE "partition-topic-1"
(
    boxid bigint PRIMARY KEY,
    packageid bigint NOT NULL
);

CREATE INDEX "idx_partition-topic-1"
ON "partition-topic-1"(packageid);

CREATE TABLE "partition-topic-1-offset"
(
    topic_offset bigint PRIMARY KEY
);
CREATE TABLE "partition-topic-2"
(
    boxid bigint PRIMARY KEY,
    packageid bigint NOT NULL
);

CREATE INDEX "idx_partition-topic-2"
ON "partition-topic-2"(packageid);

CREATE TABLE "partition-topic-2-offset"
(
    topic_offset bigint PRIMARY KEY
);

ALTER TABLE "partition-topic-0" OWNER TO box;
ALTER TABLE "partition-topic-1" OWNER TO box;
ALTER TABLE "partition-topic-2" OWNER TO box;

Insert into "partition-topic-0"(boxid,packageid) values( 0,0);
Insert into "partition-topic-0"(boxid,packageid) values( 1,0);
Insert into "partition-topic-1"(boxid,packageid) values( 2,0);
Insert into "partition-topic-1"(boxid,packageid) values( 3,0);
Insert into "partition-topic-2"(boxid,packageid) values( 4,0);
Insert into "partition-topic-2"(boxid,packageid) values( 5,0);
			
Insert into "partition-topic-0-offset"(topic_offset) values( 0);
Insert into "partition-topic-1-offset"(topic_offset) values( 0);
Insert into "partition-topic-2-offset"(topic_offset) values( 0);

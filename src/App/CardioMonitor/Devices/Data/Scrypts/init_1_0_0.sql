CREATE TABLE public."DeviceConfigurations"
(
    "ConfigId" uuid NOT NULL,
    "ConfigName" text NOT NULL,
    "DeviceId" uuid NOT NULL,
    "DeviceTypeId" uuid NOT NULL,
    "ParamsJson" json,
    PRIMARY KEY ("ConfigId")
)
WITH (
    OIDS = FALSE
);

ALTER TABLE public."DeviceConfigurations"
    OWNER to postgres;
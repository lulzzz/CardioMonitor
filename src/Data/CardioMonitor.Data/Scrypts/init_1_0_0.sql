-- Table: public."Version"

-- DROP TABLE public."Version";

CREATE TABLE public."Version"
(
  version text
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public."Version"
  OWNER TO postgres;


-- Table: public."Patients"

-- DROP TABLE public."Patients";

CREATE TABLE public."Patients"
(
  "Id" serial  NOT NULL,
  "LastName" text,
  "FirstName" text,
  "PatronymicName" text,
  "BirthDate" date,
  CONSTRAINT "Patients_PK" PRIMARY KEY ("Id")
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public."Patients"
  OWNER TO postgres;



-- Table: public."Sessions"

-- DROP TABLE public."Sessions";

CREATE TABLE public."Sessions"
(
  "Id" serial  NOT NULL,
  "DateTime" date,
  "Status" smallint,
  "PatientId" integer,
  CONSTRAINT "Session_PK" PRIMARY KEY ("Id"),
  CONSTRAINT "Session_Patient_FK" FOREIGN KEY ("PatientId")
      REFERENCES public."Patients" ("Id") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public."Sessions"
  OWNER TO postgres;

-- Table: public."SessionCycles"

-- DROP TABLE public."SessionCycles";

CREATE TABLE public."SessionCycles"
(
  "Id" serial  NOT NULL,
  "CycleNumber" integer,
  "SessionId" integer,
  CONSTRAINT "SessionCycle_PK" PRIMARY KEY ("Id"),
  CONSTRAINT "SessionCycle_Session_FK" FOREIGN KEY ("SessionId")
      REFERENCES public."Sessions" ("Id") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public."SessionCycles"
  OWNER TO postgres;

  -- Table: public."PatientParams"

-- DROP TABLE public."PatientParams";

CREATE TABLE public."PatientParams"
(
  "Id" bigserial NOT NULL,
  "Iteration" integer,
  "InclinationAngle" double precision,
  "HeartRate" smallint,
  "RepsirationRate" smallint,
  "Spo2" smallint,
  "SystolicArterialPressure" smallint,
  "DiastolicArterialPressure" smallint,
  "AverageArterialPressure" smallint,
  "SessionCycleId" integer,
  CONSTRAINT "PatientParams_PK" PRIMARY KEY ("Id"),
  CONSTRAINT "PateintParam_SessionCycle_FK" FOREIGN KEY ("SessionCycleId")
      REFERENCES public."SessionCycles" ("Id") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public."PatientParams"
  OWNER TO postgres;

-- Table: public."DeviceConfigurations"

-- DROP TABLE public."DeviceConfigurations";

CREATE TABLE public."DeviceConfigurations"
(
    "ConfigId" uuid NOT NULL,
    "ConfigName" text COLLATE pg_catalog."default" NOT NULL,
    "DeviceId" uuid NOT NULL,
    "DeviceTypeId" uuid NOT NULL,
    "ParamsJson" json,
    CONSTRAINT "DeviceConfigurations_pkey" PRIMARY KEY ("ConfigId")
)
WITH (
    OIDS = FALSE
)

ALTER TABLE public."DeviceConfigurations"
    OWNER to postgres;

INSERT INTO public."Version"(
            version)
    VALUES ('1.0.0');

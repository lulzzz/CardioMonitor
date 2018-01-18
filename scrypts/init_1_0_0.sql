﻿-- Table: public."Version"

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
  "Id" integer NOT NULL,
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


-- Table: public."Treatments"

-- DROP TABLE public."Treatments";

CREATE TABLE public."Treatments"
(
  "Id" integer NOT NULL,
  "PatientId" integer,
  "StartDate" date,
  CONSTRAINT "Treatmens_PK" PRIMARY KEY ("Id"),
  CONSTRAINT "Treatment_Patieint_FK" FOREIGN KEY ("PatientId")
      REFERENCES public."Patients" ("Id") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public."Treatments"
  OWNER TO postgres;

-- Table: public."Sessions"

-- DROP TABLE public."Sessions";

CREATE TABLE public."Sessions"
(
  "Id" integer NOT NULL,
  "DateTime" date,
  "Status" smallint,
  "TreatmentId" integer,
  CONSTRAINT "Session_PK" PRIMARY KEY ("Id"),
  CONSTRAINT "Session_Treatment_FK" FOREIGN KEY ("TreatmentId")
      REFERENCES public."Treatments" ("Id") MATCH SIMPLE
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
  "Id" integer NOT NULL,
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
  "Id" bigint NOT NULL,
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
GRANT ALL ON TABLE public."PatientParams" TO postgres;

INSERT INTO public."Version"(
            version)
    VALUES ('1.0.0');
import React from "react";
import { Box, FormControl, TextField, makeStyles } from "@material-ui/core";
import Autocomplete from "@material-ui/lab/Autocomplete";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";

const useStyles = makeStyles((theme) => ({
  root: {
    backgroundColor: theme.palette.background.dark,
    minHeight: "100%",
    paddingBottom: theme.spacing(3),
    paddingTop: theme.spacing(3),
  },
  card: {
    height: "100%",
    color: theme.palette.text.secondary,
    minHeight: 688,
  },
  formControl: {
    margin: theme.spacing(1),
    minWidth: 450,
    maxWidth: 450,
  },
  sortFormControl: {
    maxWidth: 450,
    minWidth: 450,
  },
  sortGroup: {
    position: "absolute",
    right: 0,
    top: -40,
  },
  server: {
    wordWrap: "word-break",
    textAlign: "left",
    color: "rgba(0, 0, 0, 0.54)",
  },
  serverHead: {
    color: "rgba(0, 0, 0, 0.83)",
    textAlign: "left",
    marginTop: 8,
    marginBottom: 3,
  },
}));

const ClientEnvSelect = ({
  ClientInfo,
  setClientInfo,
  setValid,
  setSubOpen,
}) => {
  const classes = useStyles();
  //3 options for selecting the source environment
  const srcEnvList = [
    { envName: "dfrd", id: 1 },
    // { envName: "backupPath", id: 2 },
    // { envName: "toolbox", id: 3 },
  ];
  return (
    <Box mb={2}>
      <FormControl required className={classes.formControl}>
        <Autocomplete
          id="clientEnv"
          options={srcEnvList}
          required
          value={ClientInfo}
          onChange={(event, newValue) => {
            setSubOpen(false);
            //if the user decides to change the environment, all source data previously fetched gets overwritten
            setClientInfo((prev) => {
              let update = { ...prev };
              update.id = newValue ? newValue.id : 0;
              update.envName = newValue ? newValue.envName : "";
              update.filePath = "";
              update.category = "";
              update.status = 0;
              update.clientId = -1;
              update.clientName = "";
              update.clientDb = "";
              update.adminDb = "";
              update.adminDbSrv = "";
              update.namespace = null;
              update.namespace2 = null;
              return update;
            });
            setValid(0);
          }}
          getOptionSelected={(option, value) => option.id === value.id}
          getOptionLabel={(option) =>
            option.id === 1 ? "DF R&D (Existing QA Env)" : ""
          }
          renderInput={(params) => (
            <TextField
              {...params}
              label="Client Environment"
              required
              variant="outlined"
            />
          )}
        />
      </FormControl>
    </Box>
  );
};

export default ClientEnvSelect;

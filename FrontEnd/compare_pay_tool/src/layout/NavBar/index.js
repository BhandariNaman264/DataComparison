import React from "react";
import PropTypes from "prop-types";
import {
  Box,
  Divider,
  Drawer,
  Hidden,
  List,
  Typography,
  makeStyles,
} from "@material-ui/core";
import { BarChart as ResultsIcon } from "react-feather";
import NavItem from "./NavItem";

import CompareIcon from "@material-ui/icons/Compare";

const items = [
  {
    id: "ComparisonForm",
    href: "/comparison",
    icon: CompareIcon,
    title: "Comparison Page",
  },
  {
    id: "Results",
    href: "/result",
    icon: ResultsIcon,
    title: "Results Page",
  },
];

const useStyles = makeStyles(() => ({
  mobileDrawer: {
    width: 256,
  },
  desktopDrawer: {
    flexShrink: 0,
    width: 256,
    top: 64,
    height: "calc(100% - 64px)",
  },
  avatar: {
    cursor: "pointer",
    width: 64,
    height: 64,
  },
}));

const NavBar = ({ closeDrawer, open, userInfo }) => {
  const classes = useStyles();
  const content = (
    <Box height="100%" display="flex" flexDirection="column">
      <Box alignItems="left" display="flex" flexDirection="column" p={2}>
        <Typography
          className={classes.name}
          color="textPrimary"
          align="left"
          variant="h4"
        >
          {userInfo.givenName} {userInfo.surname}
        </Typography>
        <Typography
          className={classes.name}
          color="textSecondary"
          align="left"
          variant="body2"
        >
          {userInfo.userPrincipalName}
        </Typography>
      </Box>
      <Divider />
      <Box p={2}>
        <List>
          {items.map((item) => (
            <NavItem
              id={item.id}
              href={item.href}
              key={item.title}
              title={item.title}
              icon={item.icon}
            />
          ))}
        </List>
      </Box>
    </Box>
  );

  return (
    <>
      <Hidden lgUp>
        <Drawer
          anchor="left"
          classes={{ paper: classes.mobileDrawer }}
          onClose={closeDrawer}
          open={open}
          variant="temporary"
        >
          {content}
        </Drawer>
      </Hidden>
      <Hidden mdDown>
        <Drawer
          anchor="left"
          classes={{ paper: classes.desktopDrawer }}
          open={open}
          variant="persistent"
        >
          {content}
        </Drawer>
      </Hidden>
    </>
  );
};

NavBar.propTypes = {
  closeDrawer: PropTypes.func,
  open: PropTypes.bool,
  userInfo: PropTypes.object,
};

NavBar.defaultProps = {
  closeDrawer: () => {},
  open: false,
  userInfo: {},
};

export default NavBar;

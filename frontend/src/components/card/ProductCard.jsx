import React from 'react';
import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';
import Card from '@material-ui/core/Card';
import CardActions from '@material-ui/core/CardActions';
import CardContent from '@material-ui/core/CardContent';
import Button from '@material-ui/core/Button';
import Typography from '@material-ui/core/Typography';
import CardHeader from '@material-ui/core/CardHeader';
import Avatar from '@material-ui/core/Avatar';
import IconButton from '@material-ui/core/IconButton';
import { MoreVert } from '@material-ui/icons';
import {
    iconsUrl
} from '../../variables/global';

const styles = {
  card: {
    minWidth: 275,
  },
  bullet: {
    display: 'inline-block',
    margin: '0 2px',
    transform: 'scale(0.8)',
  },
  title: {
    fontSize: 14,
  },
  pos: {
    marginBottom: 12,
  },
  logo: {
    width: 'auto',
    height: 'auto',
    //padding: '0px 7px'
  },
};



function ProductCard(props) {
  const { classes, item } = props;
  const bull = <span className={classes.bullet}>•</span>;
  const imageUrl = iconsUrl + item.split('-')[0].toLowerCase() + '.png'

  return (
    <Card className={classes.card}>
      <CardContent>
      <CardHeader
          avatar={
            <Avatar aria-label="Recipe" src={imageUrl} className={classes.avatar}/>

          }
          action={
            <IconButton>
              <MoreVert />
            </IconButton>
          }
          title={item}
          subheader="$100.00"
        />
        <Typography variant="h5" component="h2">
          BTC-USD
        </Typography>
        <Typography className={classes.title} color="textSecondary" gutterBottom>
          Word of the Day
        </Typography>
        
        <Typography className={classes.pos} color="textSecondary" >
          {bull} 5-min change
        </Typography>
        <Typography className={classes.pos} color="textSecondary">
          {bull} 1-hour change
        </Typography>
        <Typography className={classes.pos} color="textSecondary" >
          {bull} Something else
        </Typography>
        <Typography className={classes.pos} color="textSecondary">
          {bull} adjective
        </Typography>
        <Typography component="p">
          well meaning and kindly.
          <br />
          {'"a benevolent smile"'}
        </Typography>
      </CardContent>
      <CardActions>
        <Button size="small">Learn More</Button>
      </CardActions>
    </Card>
  );
}

ProductCard.propTypes = {
  classes: PropTypes.object.isRequired,
};

export default withStyles(styles)(ProductCard);
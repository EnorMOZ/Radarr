import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import MovieHeadshot from 'Movie/MovieHeadshot';
import styles from './MovieCastRow.css';

class MovieCastRow extends Component {

  //
  // Render

  render() {
    const {
      name,
      character,
      images
    } = this.props;

    return (
      <TableRow>

        <TableRowCell>
          <MovieHeadshot
            className={styles.poster}
            images={images}
            size={20}
            lazy={false}
          />
        </TableRowCell>

        <TableRowCell>
          {name}
        </TableRowCell>

        <TableRowCell>
          {character}
        </TableRowCell>

      </TableRow>
    );
  }
}

MovieCastRow.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  character: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default MovieCastRow;

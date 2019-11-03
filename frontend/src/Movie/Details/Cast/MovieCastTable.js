import React from 'react';
import MovieCastTableContent from './MovieCastTableContent';

function MovieCastTable(props) {
  const {
    ...otherProps
  } = props;

  return (
    <MovieCastTableContent
      {...otherProps}
    />
  );
}

MovieCastTable.propTypes = {
};

export default MovieCastTable;
